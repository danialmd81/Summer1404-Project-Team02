using System.Net;
using ETL.Infrastructure.Security;
using ETL.Infrastructure.HttpClientFixture.Fixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.Security;

[Collection("HttpClient collection")]
public class AuthCredentialValidatorTests
{
    private readonly HttpClientTestFixture _fixture;
    private readonly IConfiguration _configuration;
    private readonly AuthCredentialValidator _sut;

    public AuthCredentialValidatorTests(HttpClientTestFixture fixture)
    {
        _fixture = fixture;

        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:Authority"].Returns("https://fake-auth");
        _configuration["Authentication:ClientId"].Returns("client-id");
        _configuration["Authentication:ClientSecret"].Returns("client-secret");

        var httpFactory = Substitute.For<IHttpClientFactory>();
        httpFactory.CreateClient().Returns(_fixture.Client);

        _sut = new AuthCredentialValidator(httpFactory, _configuration);
    }

    // Constructor null checks
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenHttpClientFactoryIsNull()
    {
        Action act = () => new AuthCredentialValidator(null!, _configuration);
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClientFactory");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        Action act = () => new AuthCredentialValidator(Substitute.For<IHttpClientFactory>(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public async Task ValidateCredentialsAsync_ShouldReturnTrue_WhenResponseIsSuccessful()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.OK, "{}");

        // Act
        var result = await _sut.ValidateCredentialsAsync("user", "pass");

        // Assert
        result.Should().BeTrue();
        _fixture.Handler.LastRequest!.RequestUri!.ToString()
            .Should().Contain("/protocol/openid-connect/token");
        _fixture.Handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_ShouldReturnFalse_WhenResponseIsFailure()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.Unauthorized, "invalid creds");

        // Act
        var result = await _sut.ValidateCredentialsAsync("user", "wrongpass");

        // Assert
        result.Should().BeFalse();
    }
}
