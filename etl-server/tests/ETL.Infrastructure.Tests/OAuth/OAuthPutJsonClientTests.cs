using System.Net;
using ETL.Application.Abstractions.Security;
using ETL.Infrastructure.OAuth;
using ETL.Infrastructure.Tests.HttpClientFixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.OAuth;

[Collection("HttpClient collection")]
public class OAuthPutJsonClientTests
{
    private readonly HttpClientTestFixture _fixture;
    private readonly IAdminTokenService _adminTokenService;
    private readonly IConfiguration _configuration;
    private readonly OAuthPutJsonClient _sut;

    public OAuthPutJsonClientTests(HttpClientTestFixture fixture)
    {
        _fixture = fixture;

        _adminTokenService = Substitute.For<IAdminTokenService>();
        _adminTokenService.GetAdminAccessTokenAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("fake-token"));

        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:KeycloakBaseUrl"].Returns("https://fake.keycloak");

        var httpFactory = Substitute.For<IHttpClientFactory>();
        httpFactory.CreateClient().Returns(_fixture.Client);

        _sut = new OAuthPutJsonClient(httpFactory, _configuration, _adminTokenService);
    }

    // Constructor null-check tests
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenHttpFactoryIsNull()
    {
        Action act = () => new OAuthPutJsonClient(null!, _configuration, _adminTokenService);
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpFactory");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        Action act = () => new OAuthPutJsonClient(Substitute.For<IHttpClientFactory>(), null!, _adminTokenService);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenAdminTokenServiceIsNull()
    {
        Action act = () => new OAuthPutJsonClient(Substitute.For<IHttpClientFactory>(), _configuration, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("adminTokenService");
    }

    // Test for successful PUT
    [Fact]
    public async Task PutJsonAsync_ShouldReturnSuccess_WhenHttpResponseIsSuccessful()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.OK, "{}");

        var content = new { Name = "test" };

        // Act
        var result = await _sut.PutJsonAsync("/users/123", content);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _fixture.Handler.LastRequest!.Method.Should().Be(HttpMethod.Put);
        _fixture.Handler.LastRequest!.RequestUri!.ToString().Should().Contain("/users/123");
    }

    // Test for failure response
    [Fact]
    public async Task PutJsonAsync_ShouldReturnFailure_WhenHttpResponseIsNotFound()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.NotFound, "not found");

        var content = new { Name = "test" };

        // Act
        var result = await _sut.PutJsonAsync("/missing", content);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("/missing");
    }

    [Fact]
    public async Task PutJsonAsync_ShouldReturnFailure_WhenHttpResponseIsError()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.InternalServerError, "server error");

        var content = new { Name = "test" };

        // Act
        var result = await _sut.PutJsonAsync("/users/123", content);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("InternalServerError");
    }
}