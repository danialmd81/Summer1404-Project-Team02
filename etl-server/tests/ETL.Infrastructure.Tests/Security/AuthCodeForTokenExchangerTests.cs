using System.Net;
using System.Text.Json;
using ETL.Application.Common.DTOs;
using ETL.Infrastructure.Security;
using ETL.Infrastructure.Tests.HttpClientFixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.Security
{
    [Collection("HttpClient collection")]
    public class AuthCodeForTokenExchangerTests
    {
        private readonly HttpClientTestFixture _fixture;
        private readonly IConfiguration _configuration;
        private readonly AuthCodeForTokenExchanger _sut;

        public AuthCodeForTokenExchangerTests(HttpClientTestFixture fixture)
        {
            _fixture = fixture;

            _configuration = Substitute.For<IConfiguration>();
            _configuration["Authentication:Authority"].Returns("https://fake-auth");
            _configuration["Authentication:ClientId"].Returns("client-id");
            _configuration["Authentication:ClientSecret"].Returns("client-secret");
            _configuration["Authentication:RedirectUri"].Returns("https://fake-redirect");

            var httpFactory = Substitute.For<IHttpClientFactory>();
            httpFactory.CreateClient().Returns(_fixture.Client);

            _sut = new AuthCodeForTokenExchanger(httpFactory, _configuration);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenHttpClientFactoryIsNull()
        {
            System.Action act = () => new AuthCodeForTokenExchanger(null!, _configuration);
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpClientFactory");
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
        {
            System.Action act = () => new AuthCodeForTokenExchanger(Substitute.For<IHttpClientFactory>(), null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
        }

        [Fact]
        public async Task ExchangeCodeForTokensAsync_ShouldReturnSuccessResult_WhenResponseIsSuccessful()
        {
            // Arrange
            var expectedResponse = new TokenResponse
            {
                AccessToken = "access",
                RefreshToken = "refresh",
                AccessExpiresIn = 3600
            };
            var responseBody = JsonSerializer.Serialize(expectedResponse);

            _fixture.Handler.SetupResponse(HttpStatusCode.OK, responseBody);

            // Act
            var result = await _sut.ExchangeCodeForTokensAsync("auth-code", "callback", CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.AccessToken.Should().Be("access");
            result.Value.RefreshToken.Should().Be("refresh");

            _fixture.Handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
            _fixture.Handler.LastRequest!.RequestUri!.ToString()
                .Should().Contain("/protocol/openid-connect/token");
        }

        [Fact]
        public async Task ExchangeCodeForTokensAsync_ShouldReturnFailureResult_WhenResponseIsFailure()
        {
            // Arrange
            _fixture.Handler.SetupResponse(HttpStatusCode.BadRequest, "bad request");

            // Act
            var result = await _sut.ExchangeCodeForTokensAsync("auth-code", "callback", CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Auth.TokenExchangeFailed");
            result.Error.Description.Should().Contain("bad request");
        }
    }
}
