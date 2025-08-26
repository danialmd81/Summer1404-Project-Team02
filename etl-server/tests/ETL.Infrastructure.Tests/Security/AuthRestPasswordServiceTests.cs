using System.Net;
using System.Text.Json;
using ETL.Application.Abstractions.Security;
using ETL.Infrastructure.Security;
using ETL.Infrastructure.Tests.HttpClientFixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.Security;

[Collection("HttpClient collection")]
public class AuthRestPasswordServiceTests
{
    private readonly HttpClientTestFixture _fixture;
    private readonly IConfiguration _configuration;
    private readonly IAdminTokenService _adminTokenService;
    private readonly AuthRestPasswordService _sut;

    public AuthRestPasswordServiceTests(HttpClientTestFixture fixture)
    {
        _fixture = fixture;

        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:KeycloakBaseUrl"].Returns("https://fake.keycloak");
        _configuration["Authentication:Realm"].Returns("fake-realm");

        _adminTokenService = Substitute.For<IAdminTokenService>();
        _adminTokenService.GetAdminAccessTokenAsync(Arg.Any<CancellationToken>())
            .Returns("fake-admin-token");

        var httpFactory = Substitute.For<IHttpClientFactory>();
        httpFactory.CreateClient().Returns(_fixture.Client);

        _sut = new AuthRestPasswordService(httpFactory, _configuration, _adminTokenService);
    }

    // Constructor guard clauses
    [Fact]
    public void Constructor_ShouldThrow_WhenHttpClientFactoryIsNull()
    {
        Action act = () => new AuthRestPasswordService(null!, _configuration, _adminTokenService);
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClientFactory");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenConfigurationIsNull()
    {
        Action act = () => new AuthRestPasswordService(Substitute.For<IHttpClientFactory>(), null!, _adminTokenService);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAdminTokenServiceIsNull()
    {
        Action act = () => new AuthRestPasswordService(Substitute.For<IHttpClientFactory>(), _configuration, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("adminTokenService");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldThrow_WhenAdminTokenIsEmpty()
    {
        // Arrange
        _adminTokenService.GetAdminAccessTokenAsync(Arg.Any<CancellationToken>())
            .Returns("");

        // Act
        var act = async () => await _sut.ResetPasswordAsync("user123", "newPassword");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Could not obtain admin credentials*");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldSucceed_WhenResponseIsSuccessful()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.NoContent, "");

        // Act
        var act = async () => await _sut.ResetPasswordAsync("user123", "newPassword");

        // Assert
        await act.Should().NotThrowAsync();
        _fixture.Handler.LastRequest!.Method.Should().Be(HttpMethod.Put);
        _fixture.Handler.LastRequest!.RequestUri!.ToString()
            .Should().Contain("/users/user123/reset-password");
        _fixture.Handler.LastRequest!.Headers.Authorization!.Scheme.Should().Be("Bearer");
        _fixture.Handler.LastRequest!.Headers.Authorization!.Parameter.Should().Be("fake-admin-token");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldThrow_WhenResponseFails()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.BadRequest, "reset error");

        // Act
        var act = async () => await _sut.ResetPasswordAsync("user123", "newPassword");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*reset error*");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldSendCorrectPayload()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.NoContent, "");

        var userId = "user123";
        var newPassword = "myNewSecret123";

        // Act
        await _sut.ResetPasswordAsync(userId, newPassword);

        // Assert
        var body = await _fixture.Handler.LastRequest!.Content!.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.GetProperty("type").GetString().Should().Be("password");
        root.GetProperty("temporary").GetBoolean().Should().BeFalse();
        root.GetProperty("value").GetString().Should().Be(newPassword);
    }
}
