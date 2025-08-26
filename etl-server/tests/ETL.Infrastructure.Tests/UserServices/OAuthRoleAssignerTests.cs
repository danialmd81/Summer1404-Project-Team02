using System.Text.Json;
using ETL.Application.Common;
using ETL.Infrastructure.OAuth.Abstractions;
using ETL.Infrastructure.UserServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.UserServices;

public class OAuthRoleAssignerTests
{
    private readonly IOAuthGetJson _getJson;
    private readonly IOAuthPostJson _postJson;
    private readonly IConfiguration _configuration;
    private readonly OAuthRoleAssigner _sut;

    public OAuthRoleAssignerTests()
    {
        _getJson = Substitute.For<IOAuthGetJson>();
        _postJson = Substitute.For<IOAuthPostJson>();
        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:Realm"].Returns("myrealm");

        _sut = new OAuthRoleAssigner(_getJson, _postJson, _configuration);
    }

    // Constructor null-checks
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenGetJsonIsNull()
    {
        Action act = () => new OAuthRoleAssigner(null!, _postJson, _configuration);
        act.Should().Throw<ArgumentNullException>().WithParameterName("getJson");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenPostJsonIsNull()
    {
        Action act = () => new OAuthRoleAssigner(_getJson, null!, _configuration);
        act.Should().Throw<ArgumentNullException>().WithParameterName("postJson");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        Action act = () => new OAuthRoleAssigner(_getJson, _postJson, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldReturnSuccess_WhenRoleNameIsEmpty()
    {
        // Arrange
        var userId = "u1";
        var roleName = "";

        // Act
        var result = await _sut.AssignRoleAsync(userId, roleName);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldReturnFailure_WhenGetRoleFailsWithNotFound()
    {
        // Arrange
        var userId = "u1";
        var roleName = "admin";

        _getJson.GetJsonAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<JsonElement>(Error.NotFound("err", "msg"))));

        // Act
        var result = await _sut.AssignRoleAsync(userId, roleName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OAuth.RoleNotFound");
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldReturnFailure_WhenGetRoleFailsWithOtherError()
    {
        // Arrange
        var userId = "u1";
        var roleName = "admin";

        var error = Error.Problem("err", "msg");
        _getJson.GetJsonAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<JsonElement>(error)));

        // Act
        var result = await _sut.AssignRoleAsync(userId, roleName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldReturnFailure_WhenPostAssignFails()
    {
        // Arrange
        var userId = "u1";
        var roleName = "admin";

        var roleJson = JsonDocument.Parse("{\"id\":\"role-id\"}").RootElement;

        _getJson.GetJsonAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(roleJson)));


        var postError = Error.Problem("err", "msg");
        _postJson.PostJsonAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(postError));

        // Act
        var result = await _sut.AssignRoleAsync(userId, roleName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(postError);
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldReturnSuccess_WhenRoleIsAssigned()
    {
        // Arrange
        var userId = "u1";
        var roleName = "admin";

        var roleJson = JsonDocument.Parse("{\"id\":\"role-id\"}").RootElement;

        _getJson.GetJsonAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(roleJson)));


        _postJson.PostJsonAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _sut.AssignRoleAsync(userId, roleName);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldCallCorrectPaths()
    {
        // Arrange
        var userId = "u1";
        var roleName = "admin";

        var roleJson = JsonDocument.Parse("{\"id\":\"role-id\"}").RootElement;

        _getJson.GetJsonAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(roleJson)));

        _postJson.PostJsonAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _sut.AssignRoleAsync(userId, roleName);

        // Assert
        await _getJson.Received(1).GetJsonAsync(
            Arg.Is<string>(s => s.Contains($"/roles/{roleName}")),
            Arg.Any<CancellationToken>());

        await _postJson.Received(1).PostJsonAsync(
            Arg.Is<string>(s => s.Contains($"/users/{userId}/role-mappings/realm")),
            Arg.Any<object>(),
            Arg.Any<CancellationToken>());
    }
}