using System.Net;
using ETL.Application.Abstractions.Security;
using ETL.Infrastructure.OAuth;
using ETL.Infrastructure.Tests.OAuth.Fixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.OAuth;

[Collection("HttpClient collection")]
public class OAuthGetJsonClientTests
{
    private readonly HttpClientTestFixture _fixture;
    private readonly IAdminTokenService _adminTokenService;
    private readonly IConfiguration _configuration;
    private readonly OAuthGetJsonClient _sut;

    public OAuthGetJsonClientTests(HttpClientTestFixture fixture)
    {
        _fixture = fixture;

        _adminTokenService = Substitute.For<IAdminTokenService>();
        _adminTokenService.GetAdminAccessTokenAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("fake-token"));

        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:KeycloakBaseUrl"].Returns("https://fake.keycloak");

        var httpFactory = Substitute.For<IHttpClientFactory>();
        httpFactory.CreateClient().Returns(_fixture.Client);

        _sut = new OAuthGetJsonClient(httpFactory, _configuration, _adminTokenService);
    }

    [Fact]
    public async Task GetJsonAsync_ShouldReturnSuccess_WhenHttpResponseIsValidJson()
    {
        // Arrange
        var json = "{\"id\":123,\"name\":\"test\"}";
        _fixture.Handler.SetupResponse(HttpStatusCode.OK, json);

        // Act
        var result = await _sut.GetJsonAsync("/user/123");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.GetProperty("id").GetInt32().Should().Be(123);
        result.Value.GetProperty("name").GetString().Should().Be("test");

        _fixture.Handler.LastRequest!.Method.Should().Be(HttpMethod.Get);
        _fixture.Handler.LastRequest!.RequestUri!.ToString().Should().Contain("/user/123");
    }

    [Fact]
    public async Task GetJsonAsync_ShouldReturnFailure_WhenHttpResponseIsNotFound()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.NotFound, "not found");

        // Act
        var result = await _sut.GetJsonAsync("/missing");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OAuth.NotFound");
        result.Error.Description.Should().Contain("not found");
    }

    [Fact]
    public async Task GetJsonAsync_ShouldReturnFailure_WhenHttpResponseIsFailure()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.BadRequest, "bad request");

        // Act
        var result = await _sut.GetJsonAsync("/user/123");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OAuth.RequestFailed");
        result.Error.Description.Should().Contain("bad request");
    }

    [Fact]
    public async Task GetJsonAsync_ShouldReturnFailure_WhenAdminTokenIsMissing()
    {
        // Arrange
        _adminTokenService.GetAdminAccessTokenAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>(null));

        // Act
        var result = await _sut.GetJsonAsync("/user/123");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OAuth.AdminTokenMissing");
    }

    // Constructor null-check tests
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenHttpFactoryIsNull()
    {
        Action act = () => new OAuthGetJsonClient(null!, _configuration, _adminTokenService);
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpFactory");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        Action act = () => new OAuthGetJsonClient(Substitute.For<IHttpClientFactory>(), null!, _adminTokenService);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenAdminTokenServiceIsNull()
    {
        Action act = () => new OAuthGetJsonClient(Substitute.For<IHttpClientFactory>(), _configuration, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("adminTokenService");
    }
}