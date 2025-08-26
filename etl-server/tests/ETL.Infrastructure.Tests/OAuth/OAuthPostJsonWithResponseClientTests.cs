using System.Net;
using ETL.Application.Abstractions.Security;
using ETL.Infrastructure.OAuth;
using ETL.Infrastructure.Tests.HttpClientFixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.OAuth;

[Collection("HttpClient collection")]
public class OAuthPostJsonWithResponseClientTests
{
    private readonly HttpClientTestFixture _fixture;
    private readonly IAdminTokenService _adminTokenService;
    private readonly IConfiguration _configuration;
    private readonly OAuthPostJsonWithResponseClient _sut;

    public OAuthPostJsonWithResponseClientTests(HttpClientTestFixture fixture)
    {
        _fixture = fixture;

        _adminTokenService = Substitute.For<IAdminTokenService>();
        _adminTokenService.GetAdminAccessTokenAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("fake-token"));

        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:KeycloakBaseUrl"].Returns("https://fake.keycloak");

        var httpFactory = Substitute.For<IHttpClientFactory>();
        httpFactory.CreateClient().Returns(_fixture.Client);

        _sut = new OAuthPostJsonWithResponseClient(httpFactory, _configuration, _adminTokenService);
    }

    [Fact]
    public async Task PostJsonForResponseAsync_ShouldReturnSuccess_WhenHttpResponseIsSuccessful()
    {
        // Arrange
        var content = new { Name = "test" };
        _fixture.Handler.SetupResponse(HttpStatusCode.OK, "{}");

        // Act
        var result = await _sut.PostJsonForResponseAsync("/test", content);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StatusCode.Should().Be(HttpStatusCode.OK);
        _fixture.Handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        _fixture.Handler.LastRequest!.RequestUri!.ToString().Should().Contain("/test");
    }

    [Fact]
    public async Task PostJsonForResponseAsync_ShouldReturnFailure_WhenAdminTokenFails()
    {
        // Arrange
        _adminTokenService.GetAdminAccessTokenAsync(default).Returns(Task.FromResult<string?>(null));
        var content = new { Name = "test" };

        // Act
        var result = await _sut.PostJsonForResponseAsync("/test", content);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OAuth.AdminTokenMissing");
    }

    [Fact]
    public async Task PostJsonForResponseAsync_ShouldReturnResponse_WhenHttpResponseIsError()
    {
        // Arrange
        var content = new { Name = "test" };
        _fixture.Handler.SetupResponse(HttpStatusCode.BadRequest, "bad request");

        // Act
        var result = await _sut.PostJsonForResponseAsync("/test", content);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Constructor null-check tests
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenHttpFactoryIsNull()
    {
        Action act = () => new OAuthPostJsonWithResponseClient(null!, _configuration, _adminTokenService);
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpFactory");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        Action act = () => new OAuthPostJsonWithResponseClient(Substitute.For<IHttpClientFactory>(), null!, _adminTokenService);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenAdminTokenServiceIsNull()
    {
        Action act = () => new OAuthPostJsonWithResponseClient(Substitute.For<IHttpClientFactory>(), _configuration, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("adminTokenService");
    }

}