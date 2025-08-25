using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ETL.Application.Common.DTOs;
using ETL.Infrastructure.Security;
using ETL.Infrastructure.HttpClientFixture.Fixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.Security;

[Collection("HttpClient collection")]
public class AdminTokenServiceTests
{
    private readonly HttpClientTestFixture _fixture;
    private readonly IConfiguration _configuration;
    private readonly AdminTokenService _sut;

    public AdminTokenServiceTests(HttpClientTestFixture fixture)
    {
        _fixture = fixture;

        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:Authority"].Returns("https://fake-auth");
        _configuration["KeycloakAdmin:ClientId"].Returns("client-id");
        _configuration["KeycloakAdmin:ClientSecret"].Returns("client-secret");

        var httpFactory = Substitute.For<IHttpClientFactory>();
        httpFactory.CreateClient().Returns(_fixture.Client);

        _sut = new AdminTokenService(httpFactory, _configuration);
    }

    // Constructor null checks
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenHttpClientFactoryIsNull()
    {
        Action act = () => new AdminTokenService(null!, _configuration);
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClientFactory");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        Action act = () => new AdminTokenService(Substitute.For<IHttpClientFactory>(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public async Task GetAdminAccessTokenAsync_ShouldReturnToken_WhenResponseIsSuccessful()
    {
        // Arrange
        var tokenResponse = new TokenResponse { AccessToken = "fake-token" };
        var responseBody = JsonSerializer.Serialize(tokenResponse);

        _fixture.Handler.SetupResponse(HttpStatusCode.OK, responseBody);

        // Act
        var token = await _sut.GetAdminAccessTokenAsync();

        // Assert
        token.Should().Be("fake-token");
        _fixture.Handler.LastRequest!.RequestUri!.ToString()
            .Should().Contain("/protocol/openid-connect/token");
        _fixture.Handler.LastRequest!.Method.Should().Be(HttpMethod.Post); // extra assert
    }

    [Fact]
    public async Task GetAdminAccessTokenAsync_ShouldThrow_WhenResponseIsFailure()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.BadRequest, "bad request");

        // Act
        Func<Task> act = () => _sut.GetAdminAccessTokenAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to obtain admin access token from OAuth*");
    }
}