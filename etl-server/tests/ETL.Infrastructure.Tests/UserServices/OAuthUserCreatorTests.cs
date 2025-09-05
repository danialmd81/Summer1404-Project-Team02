using System.Net;
using ETL.Application.Common;
using ETL.Application.User.Create;
using ETL.Infrastructure.OAuthClients.Abstractions;
using ETL.Infrastructure.UserServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.UserServices;

public class OAuthUserCreatorTests
{
    private readonly IOAuthPostJsonWithResponse _postWithResponse;
    private readonly IConfiguration _configuration;
    private readonly OAuthUserCreator _sut;

    public OAuthUserCreatorTests()
    {
        _postWithResponse = Substitute.For<IOAuthPostJsonWithResponse>();

        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:Realm"].Returns("myrealm");

        _sut = new OAuthUserCreator(_postWithResponse, _configuration);
    }

    // Constructor null-checks
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenPostWithResponseIsNull()
    {
        Action act = () => new OAuthUserCreator(null!, _configuration);
        act.Should().Throw<ArgumentNullException>().WithParameterName("postWithResponse");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        Action act = () => new OAuthUserCreator(_postWithResponse, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnSuccess_WhenResponseIsSuccessful()
    {
        // Arrange
        var command = new CreateUserCommand("u1", "", "", "", "p1", "");

        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Headers = { Location = new Uri("https://fake/users/123") }
        };

        _postWithResponse.PostJsonForResponseAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        // Act
        var result = await _sut.CreateUserAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("123");
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnFailure_WhenResponseIsFailure()
    {
        // Arrange
        var command = new CreateUserCommand("u1", "", "", "", "p1", "");

        _postWithResponse.PostJsonForResponseAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<HttpResponseMessage>(Error.Problem("err", "msg")));

        // Act
        var result = await _sut.CreateUserAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("err");
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnFailure_WhenLocationHeaderIsMissing()
    {
        // Arrange
        var command = new CreateUserCommand("u1", "", "", "", "p1", "");
        var response = new HttpResponseMessage(HttpStatusCode.Created); // no Location header

        _postWithResponse.PostJsonForResponseAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        // Act
        var result = await _sut.CreateUserAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OAuth.NoLocationHeader");
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnFailure_WhenStatusCodeIsNotSuccessful()
    {
        // Arrange
        var command = new CreateUserCommand("u1", "", "", "", "p1", "");
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("bad request")
        };

        _postWithResponse.PostJsonForResponseAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        // Act
        var result = await _sut.CreateUserAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("BadRequest");
    }
}