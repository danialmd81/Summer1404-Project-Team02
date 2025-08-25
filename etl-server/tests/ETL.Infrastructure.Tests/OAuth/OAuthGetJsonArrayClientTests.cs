using System.Net;
using ETL.Application.Abstractions.Security;
using ETL.Infrastructure.OAuth;
using ETL.Infrastructure.Tests.OAuth.Fixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.OAuth;

[Collection("HttpClient collection")]
public class OAuthGetJsonArrayClientTests
{
    private readonly HttpClientTestFixture _fixture;
    private readonly IAdminTokenService _adminTokenService;
    private readonly IConfiguration _configuration;
    private readonly OAuthGetJsonArrayClient _sut;

    public OAuthGetJsonArrayClientTests(HttpClientTestFixture fixture)
    {
        _fixture = fixture;

        _adminTokenService = Substitute.For<IAdminTokenService>();
        _adminTokenService.GetAdminAccessTokenAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("fake-token"));

        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:KeycloakBaseUrl"].Returns("https://fake.keycloak");

        var httpFactory = Substitute.For<IHttpClientFactory>();
        httpFactory.CreateClient().Returns(_fixture.Client);

        _sut = new OAuthGetJsonArrayClient(httpFactory, _configuration, _adminTokenService);
    }

    [Fact]
    public async Task GetJsonArrayAsync_ShouldReturnSuccess_WhenHttpResponseIsArray()
    {
        // Arrange
        var jsonArray = "[{\"id\":1},{\"id\":2}]";
        _fixture.Handler.SetupResponse(HttpStatusCode.OK, jsonArray);

        // Act
        var result = await _sut.GetJsonArrayAsync("/users");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].GetProperty("id").GetInt32().Should().Be(1);
        result.Value[1].GetProperty("id").GetInt32().Should().Be(2);

        _fixture.Handler.LastRequest!.Method.Should().Be(HttpMethod.Get);
        _fixture.Handler.LastRequest!.RequestUri.ToString().Should().Contain("/users");
    }

    [Fact]
    public async Task GetJsonArrayAsync_ShouldReturnSuccess_WithEmptyList_WhenResponseIsEmptyArray()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.OK, "[]");

        // Act
        var result = await _sut.GetJsonArrayAsync("/empty");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetJsonArrayAsync_ShouldReturnFailure_WhenHttpResponseIsFailure()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.BadRequest, "bad request");

        // Act
        var result = await _sut.GetJsonArrayAsync("/users");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OAuth.RequestFailed");
        result.Error.Description.Should().Contain("bad request");
    }

    [Fact]
    public async Task GetJsonArrayAsync_ShouldReturnFailure_WhenResourceNotFound()
    {
        // Arrange
        _fixture.Handler.SetupResponse(HttpStatusCode.NotFound, "not found");

        // Act
        var result = await _sut.GetJsonArrayAsync("/missing");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OAuth.NotFound");
        result.Error.Description.Should().Contain("/missing");
    }

    [Fact]
    public async Task GetJsonArrayAsync_ShouldReturnFailure_WhenAdminTokenIsMissing()
    {
        // Arrange
        _adminTokenService.GetAdminAccessTokenAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>(null));

        // Act
        var result = await _sut.GetJsonArrayAsync("/users");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OAuth.AdminTokenMissing");
    }
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenHttpFactoryIsNull()
    {
        // Act
        Action act = () => new OAuthGetJsonArrayClient(null!, _configuration, _adminTokenService);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpFactory");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
    {
        // Act
        Action act = () => new OAuthGetJsonArrayClient(Substitute.For<IHttpClientFactory>(), null!, _adminTokenService);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenAdminTokenServiceIsNull()
    {
        // Act
        Action act = () => new OAuthGetJsonArrayClient(Substitute.For<IHttpClientFactory>(), _configuration, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("adminTokenService");
    }

}