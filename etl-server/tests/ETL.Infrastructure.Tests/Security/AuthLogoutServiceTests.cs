using System.Net;
using ETL.Infrastructure.Security;
using ETL.Infrastructure.Tests.HttpClientFixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.Security;

[Collection("HttpClient collection")]
public class AuthLogoutServiceTests
{
    private readonly HttpClientTestFixture _fixture;
    private readonly IConfiguration _configuration;
    private readonly AuthLogoutService _sut;

    public AuthLogoutServiceTests(HttpClientTestFixture fixture)
    {
        _fixture = fixture;

        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:Authority"].Returns("https://fake-auth");
        _configuration["Authentication:ClientId"].Returns("client-id");
        _configuration["Authentication:ClientSecret"].Returns("client-secret");

        var httpFactory = Substitute.For<IHttpClientFactory>();
        httpFactory.CreateClient().Returns(_fixture.Client);

        _sut = new AuthLogoutService(httpFactory, _configuration);
    }

    // Constructor null checks
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenHttpClientFactoryIsNull()
    {
        Action act = () => new AuthLogoutService(null!, _configuration);
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClientFactory");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        Action act = () => new AuthLogoutService(Substitute.For<IHttpClientFactory>(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public async Task LogoutAsync_ShouldSucceed_WhenResponseIsSuccessful()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.OK, "");

        // Act
        var act = async () => await _sut.LogoutAsync("Bearer fake-access-token", "fake-refresh-token");

        // Assert
        await act.Should().NotThrowAsync();
        _fixture.Handler.LastRequest!.RequestUri!.ToString()
            .Should().Contain("/protocol/openid-connect/logout");
        _fixture.Handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        _fixture.Handler.LastRequest!.Headers.Authorization!.Scheme.Should().Be("Bearer");
        _fixture.Handler.LastRequest!.Headers.Authorization!.Parameter.Should().Be("fake-access-token");
    }

    [Fact]
    public async Task LogoutAsync_ShouldThrowInvalidOperationException_WhenResponseFails()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.BadRequest, "logout error");

        // Act
        var act = async () => await _sut.LogoutAsync("token", "refresh");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*logout error*");
    }

    [Fact]
    public async Task LogoutAsync_ShouldSendRequestWithoutAuthorizationHeader_WhenAccessTokenIsNull()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.OK, "");

        // Act
        await _sut.LogoutAsync(null, "refresh");

        // Assert
        _fixture.Handler.LastRequest!.Headers.Authorization.Should().BeNull();
    }
}
