using System.Net;
using ETL.Infrastructure.Security;
using ETL.Infrastructure.Tests.HttpClientFixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.Security
{
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

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenHttpClientFactoryIsNull()
        {
            System.Action act = () => new AuthLogoutService(null!, _configuration);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClientFactory");
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
        {
            System.Action act = () => new AuthLogoutService(Substitute.For<IHttpClientFactory>(), null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
        }

        [Fact]
        public async Task LogoutAsync_ShouldReturnSuccessResult_WhenResponseIsSuccessful()
        {
            // Arrange
            _fixture.Handler.SetupResponse(HttpStatusCode.OK, "");

            // Act
            var result = await _sut.LogoutAsync("Bearer fake-access-token", "fake-refresh-token", CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _fixture.Handler.LastRequest!.RequestUri!.ToString()
                .Should().Contain("/protocol/openid-connect/logout");
            _fixture.Handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
            _fixture.Handler.LastRequest!.Headers.Authorization!.Scheme.Should().Be("Bearer");
            _fixture.Handler.LastRequest!.Headers.Authorization!.Parameter.Should().Be("fake-access-token");
        }

        [Fact]
        public async Task LogoutAsync_ShouldReturnFailureResult_WhenResponseFails()
        {
            // Arrange
            _fixture.Handler.SetupResponse(HttpStatusCode.BadRequest, "logout error");

            // Act
            var result = await _sut.LogoutAsync("token", "refresh", CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("OAuth.LogoutFailed");
            result.Error.Description.Should().Contain("logout error");
        }

        [Fact]
        public async Task LogoutAsync_ShouldSendRequestWithoutAuthorizationHeader_WhenAccessTokenIsNull()
        {
            // Arrange
            _fixture.Handler.SetupResponse(HttpStatusCode.OK, "");

            // Act
            var result = await _sut.LogoutAsync(null, "refresh", CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _fixture.Handler.LastRequest!.Headers.Authorization.Should().BeNull();
        }
    }
}
