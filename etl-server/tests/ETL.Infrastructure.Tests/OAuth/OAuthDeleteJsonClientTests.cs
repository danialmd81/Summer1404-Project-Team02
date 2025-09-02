using System.Net;
using ETL.Application.Abstractions.Security;
using ETL.Infrastructure.OAuth;
using ETL.Infrastructure.Tests.HttpClientFixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.OAuth;

[Collection("HttpClient collection")]
public class OAuthDeleteJsonClientTests
{
    private readonly HttpClientTestFixture _fixture;
    private readonly IAdminTokenService _adminTokenService;
    private readonly IConfiguration _configuration;
    private readonly OAuthDeleteJsonClient _sut;

    public OAuthDeleteJsonClientTests(HttpClientTestFixture fixture)
    {
        _fixture = fixture;

        _adminTokenService = Substitute.For<IAdminTokenService>();
        _adminTokenService.GetAdminAccessTokenAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("fake-token"));

        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:KeycloakBaseUrl"].Returns("https://fake.keycloak");

        var httpFactory = Substitute.For<IHttpClientFactory>();
        httpFactory.CreateClient().Returns(_fixture.Client);

        _sut = new OAuthDeleteJsonClient(httpFactory, _configuration, _adminTokenService);
    }

    [Fact]
    public async Task DeleteJsonAsync_ShouldReturnSuccess_WhenHttpResponseIsSuccess()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.OK);

        // Act
        var result = await _sut.DeleteJsonAsync("/users/123");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteJsonAsync_ShouldReturnFailure_WhenHttpResponseIsFailure()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.BadRequest, "bad request");

        // Act
        var result = await _sut.DeleteJsonAsync("/users/123");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OAuth.RequestFailed");
        result.Error.Description.Should().Contain("bad request");
    }

    [Fact]
    public async Task DeleteJsonAsync_ShouldReturnFailure_WhenAdminTokenIsMissing()
    {
        // Arrange
        _adminTokenService.GetAdminAccessTokenAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>(null));

        // Act
        var result = await _sut.DeleteJsonAsync("/users/123");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OAuth.AdminTokenMissing");
    }

    [Fact]
    public async Task DeleteJsonAsync_ShouldSendContent_WhenContentIsProvided()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.OK);
        var contentObj = new { id = 123, name = "test" };

        // Act
        var result = await _sut.DeleteJsonAsync("/users/123", contentObj);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _fixture.Handler.LastRequest!.Method.Should().Be(HttpMethod.Delete);
        _fixture.Handler.LastRequest!.RequestUri!.ToString().Should().Contain("/users/123");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenHttpFactoryIsNull()
    {
        // Act
        Action act = () => new OAuthDeleteJsonClient(null!, _configuration, _adminTokenService);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpFactory");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        // Act
        Action act = () => new OAuthDeleteJsonClient(Substitute.For<IHttpClientFactory>(), null!, _adminTokenService);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenAdminTokenServiceIsNull()
    {
        // Act
        Action act = () => new OAuthDeleteJsonClient(Substitute.For<IHttpClientFactory>(), _configuration, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("adminTokenService");
    }
}