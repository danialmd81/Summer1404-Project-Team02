using System.Text.Json;
using ETL.Application.Common;
using ETL.Infrastructure.OAuth.Abstractions;
using ETL.Infrastructure.UserServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.UserServices;

public class OAuthUserDeleterTests
{
    private readonly IOAuthGetJson _getJson;
    private readonly IOAuthDeleteJson _deleteJson;
    private readonly IConfiguration _configuration;
    private readonly OAuthUserDeleter _sut;

    public OAuthUserDeleterTests()
    {
        _getJson = Substitute.For<IOAuthGetJson>();
        _deleteJson = Substitute.For<IOAuthDeleteJson>();
        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:Realm"].Returns("myrealm");

        _sut = new OAuthUserDeleter(_getJson, _deleteJson, _configuration);
    }

    // Constructor null-checks
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenGetJsonIsNull()
    {
        Action act = () => new OAuthUserDeleter(null!, _deleteJson, _configuration);
        act.Should().Throw<ArgumentNullException>().WithParameterName("getJson");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDeleteJsonIsNull()
    {
        Action act = () => new OAuthUserDeleter(_getJson, null!, _configuration);
        act.Should().Throw<ArgumentNullException>().WithParameterName("delete");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        Action act = () => new OAuthUserDeleter(_getJson, _deleteJson, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userId = "u1";
        _getJson.GetJsonAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<JsonElement>(Error.NotFound("err", "msg"))));

        // Act
        var result = await _sut.DeleteUserAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OAuth.UserNotFound");
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFailure_WhenGetJsonFailsWithOtherError()
    {
        // Arrange
        var userId = "u1";
        var error = Error.Problem("err", "msg");
        _getJson.GetJsonAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<JsonElement>(error)));

        // Act
        var result = await _sut.DeleteUserAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFailure_WhenDeleteFails()
    {
        // Arrange
        var userId = "u1";
        var json = JsonDocument.Parse("{\"id\":\"u1\"}").RootElement;

        _getJson.GetJsonAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(json)));

        var error = Error.Problem("err", "delete failed");
        _deleteJson.DeleteJsonAsync(Arg.Any<string>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure(error)));

        // Act
        var result = await _sut.DeleteUserAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnSuccess_WhenDeleteSucceeds()
    {
        // Arrange
        var userId = "u1";
        var json = JsonDocument.Parse("{\"id\":\"u1\"}").RootElement;

        _getJson.GetJsonAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(json)));

        _deleteJson.DeleteJsonAsync(Arg.Any<string>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _sut.DeleteUserAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldCallCorrectPaths()
    {
        // Arrange
        var userId = "u1";
        var json = JsonDocument.Parse("{\"id\":\"u1\"}").RootElement;

        _getJson.GetJsonAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(json)));
        _deleteJson.DeleteJsonAsync(Arg.Any<string>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        await _sut.DeleteUserAsync(userId);

        // Assert
        await _getJson.Received(1).GetJsonAsync(
            Arg.Is<string>(s => s.Contains($"/users/{userId}")),
            Arg.Any<CancellationToken>());

        await _deleteJson.Received(1).DeleteJsonAsync(
            Arg.Is<string>(s => s.Contains($"/users/{userId}")),
            Arg.Is<object?>(o => o == null),
            Arg.Any<CancellationToken>());
    }
}