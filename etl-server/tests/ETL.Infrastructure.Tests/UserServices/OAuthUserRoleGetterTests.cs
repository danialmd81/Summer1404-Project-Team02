using System.Text.Json;
using ETL.Application.Common;
using ETL.Infrastructure.OAuthClients.Abstractions;
using ETL.Infrastructure.UserServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.UserServices;

public class OAuthUserRoleGetterTests
{
    private readonly IOAuthGetJsonArray _getArray;
    private readonly IConfiguration _configuration;
    private readonly OAuthUserRoleGetter _sut;

    public OAuthUserRoleGetterTests()
    {
        _getArray = Substitute.For<IOAuthGetJsonArray>();
        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:Realm"].Returns("myrealm");

        _sut = new OAuthUserRoleGetter(_getArray, _configuration);
    }

    // Constructor null-checks
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenGetArrayIsNull()
    {
        Action act = () => new OAuthUserRoleGetter(null!, _configuration);
        act.Should().Throw<ArgumentNullException>().WithParameterName("getArray");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        Action act = () => new OAuthUserRoleGetter(_getArray, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public async Task GetRoleForUserAsync_ShouldReturnNull_WhenGetArrayFailsWithNotFound()
    {
        // Arrange
        var userId = "u1";
        var error = Error.NotFound("notfound", "not found");

        _getArray.GetJsonArrayAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<List<JsonElement>>(error)));

        // Act
        var result = await _sut.GetRoleForUserAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetRoleForUserAsync_ShouldReturnFailure_WhenGetArrayFailsWithOtherError()
    {
        // Arrange
        var userId = "u1";
        var error = Error.Problem("err", "bad request");

        _getArray.GetJsonArrayAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<List<JsonElement>>(error)));

        // Act
        var result = await _sut.GetRoleForUserAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task GetRoleForUserAsync_ShouldReturnNull_WhenArrayIsNull()
    {
        // Arrange
        var userId = "u1";
        _getArray.GetJsonArrayAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<List<JsonElement>>(null!)));

        // Act
        var result = await _sut.GetRoleForUserAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetRoleForUserAsync_ShouldReturnNull_WhenArrayIsEmpty()
    {
        // Arrange
        var userId = "u1";
        _getArray.GetJsonArrayAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new List<JsonElement>())));

        // Act
        var result = await _sut.GetRoleForUserAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetRoleForUserAsync_ShouldReturnFirstRoleName_WhenArrayContainsRole()
    {
        // Arrange
        var userId = "u1";
        var roleJson = "{\"name\":\"admin\"}";
        var element = JsonDocument.Parse(roleJson).RootElement;

        _getArray.GetJsonArrayAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new List<JsonElement> { element })));

        // Act
        var result = await _sut.GetRoleForUserAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("admin");
    }

    [Fact]
    public async Task GetRoleForUserAsync_ShouldSkipElementsWithoutNameProperty()
    {
        // Arrange
        var userId = "u1";
        var role1 = JsonDocument.Parse("{\"id\":\"123\"}").RootElement;
        var role2 = JsonDocument.Parse("{\"name\":\"editor\"}").RootElement;

        _getArray.GetJsonArrayAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new List<JsonElement> { role1, role2 })));

        // Act
        var result = await _sut.GetRoleForUserAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("editor");
    }
}