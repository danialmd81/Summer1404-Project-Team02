using ETL.Application.Common;
using ETL.Application.User.Edit;
using ETL.Infrastructure.OAuthClients.Abstractions;
using ETL.Infrastructure.UserServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.UserServices;

public class OAuthUserUpdaterTests
{
    private readonly IOAuthPutJson _putJson;
    private readonly IConfiguration _configuration;
    private readonly OAuthUserUpdater _sut;

    public OAuthUserUpdaterTests()
    {
        _putJson = Substitute.For<IOAuthPutJson>();
        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:Realm"].Returns("myrealm");

        _sut = new OAuthUserUpdater(_putJson, _configuration);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenPutJsonIsNull()
    {
        Action act = () => new OAuthUserUpdater(null!, _configuration);
        act.Should().Throw<ArgumentNullException>().WithParameterName("putJson");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenConfigurationIsNull()
    {
        Action act = () => new OAuthUserUpdater(_putJson, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnSuccess_AndNotCallPut_WhenNoFieldsProvided()
    {
        var cmd = new EditUserCommand("u1", null, null, null, null);

        var result = await _sut.UpdateUserAsync(cmd);

        result.IsSuccess.Should().BeTrue();
        await _putJson.DidNotReceiveWithAnyArgs().PutJsonAsync(default!, default!, default);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnFailure_WhenPutFails()
    {
        var cmd = new EditUserCommand("u1", "", "test@example.com", "", "");
        var error = Error.Problem("err", "failed");

        _putJson.PutJsonAsync(Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure(error)));

        var result = await _sut.UpdateUserAsync(cmd);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnSuccess_WhenPutSucceeds()
    {
        var cmd = new EditUserCommand("u1", "newuser", "", "John", "");

        _putJson.PutJsonAsync(Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        var result = await _sut.UpdateUserAsync(cmd);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldCallPutJson_WithCorrectPathAndPayload()
    {
        var cmd = new EditUserCommand("user@id", "alice", "", "", "Smith");

        Dictionary<string, object?>? capturedPayload = null;
        string? capturedPath = null;

        _putJson.PutJsonAsync(Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()))
            .AndDoes(ci =>
            {
                capturedPath = ci.ArgAt<string>(0);
                capturedPayload = ci.ArgAt<Dictionary<string, object?>>(1);
            });

        var result = await _sut.UpdateUserAsync(cmd);

        result.IsSuccess.Should().BeTrue();

        capturedPath.Should().Be("/admin/realms/myrealm/users/user%40id");
        capturedPayload.Should().ContainKey("username").WhoseValue.Should().Be("alice");
        capturedPayload.Should().ContainKey("lastName").WhoseValue.Should().Be("Smith");
        capturedPayload.Should().ContainKey("email").WhoseValue.Should().Be("");
    }
}