using System.Text.Json;
using ETL.Application.Common;
using ETL.Infrastructure.OAuth.Abstractions;
using ETL.Infrastructure.UserServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.UserServices;

public class OAuthAllUserReaderTests
{
    private readonly IOAuthGetJsonArray _getArray;
    private readonly IConfiguration _configuration;
    private readonly OAuthAllUserReader _sut;

    public OAuthAllUserReaderTests()
    {
        _getArray = Substitute.For<IOAuthGetJsonArray>();
        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:Realm"].Returns("myrealm");

        _sut = new OAuthAllUserReader(_getArray, _configuration);
    }

    // Constructor null-checks
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenGetArrayIsNull()
    {
        Action act = () => new OAuthAllUserReader(null!, _configuration);
        act.Should().Throw<ArgumentNullException>().WithParameterName("getArray");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        Action act = () => new OAuthAllUserReader(_getArray, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedUsers_WhenResponseIsSuccessful()
    {
        // Arrange
        var jsonElements = new List<JsonElement>
        {
            JsonDocument.Parse("{\"id\":\"1\",\"username\":\"u1\",\"email\":\"e1\",\"firstName\":\"f1\",\"lastName\":\"l1\"}").RootElement,
            JsonDocument.Parse("{\"id\":\"2\",\"username\":\"u2\",\"email\":\"e2\",\"firstName\":\"f2\",\"lastName\":\"l2\"}").RootElement
        };

        _getArray.GetJsonArrayAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(jsonElements));

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Id.Should().Be("1");
        result.Value[0].Username.Should().Be("u1");
        result.Value[1].Email.Should().Be("e2");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnFailure_WhenGetArrayFails()
    {
        // Arrange
        var error = Error.Problem("err", "msg");
        _getArray.GetJsonArrayAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<List<JsonElement>>(error));

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task GetAllAsync_ShouldIncludeFirstAndMaxQueryParameters_WhenProvided()
    {
        // Arrange
        var jsonElements = new List<JsonElement>();
        _getArray.GetJsonArrayAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(jsonElements));

        // Act
        await _sut.GetAllAsync(first: 5, max: 10);

        // Assert
        await _getArray.Received(1).GetJsonArrayAsync(
            Arg.Is<string>(s => s.Contains("first=5") && s.Contains("max=10")), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllAsync_ShouldHandleEmptyJsonList()
    {
        // Arrange
        _getArray.GetJsonArrayAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new List<JsonElement>()));

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}