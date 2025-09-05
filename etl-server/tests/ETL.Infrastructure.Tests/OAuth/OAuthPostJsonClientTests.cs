using System.Net;
using ETL.Application.Abstractions.Security;
using ETL.Infrastructure.OAuthClients;
using ETL.Infrastructure.Tests.HttpClientFixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.OAuth;

[Collection("HttpClient collection")]
public class OAuthPostJsonClientTests
{
    private readonly HttpClientTestFixture _fixture;
    private readonly IAdminTokenService _adminTokenService;
    private readonly IConfiguration _configuration;
    private readonly OAuthPostJsonClient _sut;

    public OAuthPostJsonClientTests(HttpClientTestFixture fixture)
    {
        _fixture = fixture;

        _adminTokenService = Substitute.For<IAdminTokenService>();
        _adminTokenService.GetAdminAccessTokenAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("fake-token"));

        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:KeycloakBaseUrl"].Returns("https://fake.keycloak");

        var httpFactory = Substitute.For<IHttpClientFactory>();
        httpFactory.CreateClient().Returns(_fixture.Client);

        _sut = new OAuthPostJsonClient(httpFactory, _configuration, _adminTokenService);
    }

    [Fact]
    public async Task PostJsonAsync_ShouldReturnSuccess_WhenHttpResponseIsSuccessful()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.OK, "{}");
        var content = new { Name = "test" };

        // Act
        var result = await _sut.PostJsonAsync("/users", content);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _fixture.Handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        _fixture.Handler.LastRequest!.RequestUri!.ToString().Should().Contain("/users");

        _fixture.Handler.LastRequestContent.Should().Contain("\"name\":\"test\"");
    }

    [Fact]
    public async Task PostJsonAsync_ShouldReturnFailure_WhenHttpResponseIsFailure()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.BadRequest, "bad request");
        var content = new { Name = "test" };

        // Act
        var result = await _sut.PostJsonAsync("/users", content);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OAuth.RequestFailed");
        result.Error.Description.Should().Contain("bad request");
    }

    [Fact]
    public async Task PostJsonAsync_ShouldReturnFailure_WhenAdminTokenIsMissing()
    {
        // Arrange
        _adminTokenService.GetAdminAccessTokenAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>(null));

        var content = new { Name = "test" };

        // Act
        var result = await _sut.PostJsonAsync("/users", content);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OAuth.AdminTokenMissing");
    }

    // Constructor null-check tests
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenHttpFactoryIsNull()
    {
        Action act = () => new OAuthPostJsonClient(null!, _configuration, _adminTokenService);
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpFactory");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        Action act = () => new OAuthPostJsonClient(Substitute.For<IHttpClientFactory>(), null!, _adminTokenService);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenAdminTokenServiceIsNull()
    {
        Action act = () => new OAuthPostJsonClient(Substitute.For<IHttpClientFactory>(), _configuration, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("adminTokenService");
    }
}