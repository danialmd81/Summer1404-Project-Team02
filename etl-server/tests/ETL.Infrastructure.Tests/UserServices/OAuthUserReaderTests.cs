using System.Text.Json;
using ETL.Application.Common;
using ETL.Infrastructure.OAuthClients.Abstractions;
using ETL.Infrastructure.UserServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.UserServices;

public class OAuthUserReaderTests
{
    private readonly IOAuthGetJson _getJson;
    private readonly IConfiguration _configuration;
    private readonly OAuthUserReader _sut;

    public OAuthUserReaderTests()
    {
        _getJson = Substitute.For<IOAuthGetJson>();
        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:Realm"].Returns("myrealm");

        _sut = new OAuthUserReader(_getJson, _configuration);
    }

    // Constructor null-checks
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenGetJsonIsNull()
    {
        Action act = () => new OAuthUserReader(null!, _configuration);
        act.Should().Throw<ArgumentNullException>().WithParameterName("getJson");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        Action act = () => new OAuthUserReader(_getJson, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFailure_WhenGetJsonFails()
    {
        // Arrange
        var userId = "u1";
        var error = Error.Problem("err", "msg");

        _getJson.GetJsonAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<JsonElement>(error)));

        // Act
        var result = await _sut.GetByIdAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMappedUser_WhenResponseIsSuccessful()
    {
        // Arrange
        var userId = "u1";
        var json = JsonDocument.Parse("{\"id\":\"u1\",\"username\":\"user1\",\"email\":\"e1\",\"firstName\":\"f1\",\"lastName\":\"l1\"}").RootElement;

        _getJson.GetJsonAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(json)));

        // Act
        var result = await _sut.GetByIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be("u1");
        result.Value.Username.Should().Be("user1");
        result.Value.Email.Should().Be("e1");
        result.Value.FirstName.Should().Be("f1");
        result.Value.LastName.Should().Be("l1");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldHandleMissingProperties()
    {
        // Arrange
        var userId = "u1";
        var json = JsonDocument.Parse("{\"id\":\"u1\"}").RootElement; // only id

        _getJson.GetJsonAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(json)));

        // Act
        var result = await _sut.GetByIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be("u1");
        result.Value.Username.Should().BeNull();
        result.Value.Email.Should().BeNull();
        result.Value.FirstName.Should().BeNull();
        result.Value.LastName.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldCallCorrectPath()
    {
        // Arrange
        var userId = "u1";
        var json = JsonDocument.Parse("{\"id\":\"u1\"}").RootElement;

        _getJson.GetJsonAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(json)));

        // Act
        await _sut.GetByIdAsync(userId);

        // Assert
        await _getJson.Received(1).GetJsonAsync(
            Arg.Is<string>(s => s.Contains($"/users/{userId}")),
            Arg.Any<CancellationToken>());
    }
}