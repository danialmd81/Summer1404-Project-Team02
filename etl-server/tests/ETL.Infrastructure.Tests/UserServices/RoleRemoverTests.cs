using System.Text.Json;
using ETL.Application.Common;
using ETL.Infrastructure.OAuth.Abstractions;
using ETL.Infrastructure.UserServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace ETL.Infrastructure.Tests.UserServices;

public class RoleRemoverTests
{
    private readonly IOAuthGetJsonArray _getArray;
    private readonly IOAuthDeleteJson _delete;
    private readonly IConfiguration _configuration;
    private readonly RoleRemover _sut;

    public RoleRemoverTests()
    {
        _getArray = Substitute.For<IOAuthGetJsonArray>();
        _delete = Substitute.For<IOAuthDeleteJson>();
        _configuration = Substitute.For<IConfiguration>();
        _configuration["Authentication:Realm"].Returns("myrealm");

        _sut = new RoleRemover(_getArray, _delete, _configuration);
    }

    // Constructor null checks
    [Fact]
    public void Constructor_ShouldThrow_WhenGetArrayIsNull()
    {
        Action act = () => new RoleRemover(null!, _delete, _configuration);
        act.Should().Throw<ArgumentNullException>().WithParameterName("getArray");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDeleteIsNull()
    {
        Action act = () => new RoleRemover(_getArray, null!, _configuration);
        act.Should().Throw<ArgumentNullException>().WithParameterName("delete");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenConfigurationIsNull()
    {
        Action act = () => new RoleRemover(_getArray, _delete, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public async Task RemoveAllRealmRolesAsync_ShouldReturnSuccess_WhenNoRoles()
    {
        // Arrange
        var userId = "u1";
        _getArray.GetJsonArrayAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<List<JsonElement>>(new List<JsonElement>())));

        // Act
        var result = await _sut.RemoveAllRealmRolesAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _delete.DidNotReceiveWithAnyArgs().DeleteJsonAsync(default!, default!, default);
    }

    [Fact]
    public async Task RemoveAllRealmRolesAsync_ShouldReturnFailure_WhenGetArrayFails()
    {
        // Arrange
        var userId = "u1";
        var error = Error.Problem("err", "fail");
        _getArray.GetJsonArrayAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<List<JsonElement>>(error)));

        // Act
        var result = await _sut.RemoveAllRealmRolesAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task RemoveAllRealmRolesAsync_ShouldReturnFailure_WhenDeleteFails()
    {
        // Arrange
        var userId = "u1";
        var roleJson = JsonDocument.Parse("{\"name\":\"admin\"}").RootElement;
        var roles = new List<JsonElement> { roleJson };
        _getArray.GetJsonArrayAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(roles)));

        var error = Error.Problem("delerr", "delete failed");
        _delete.DeleteJsonAsync(Arg.Any<string>(), Arg.Any<List<JsonElement>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure(error)));

        // Act
        var result = await _sut.RemoveAllRealmRolesAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task RemoveAllRealmRolesAsync_ShouldReturnSuccess_WhenRolesDeletedSuccessfully()
    {
        // Arrange
        var userId = "u1";
        var roleJson = JsonDocument.Parse("{\"name\":\"admin\"}").RootElement;
        var roles = new List<JsonElement> { roleJson };
        _getArray.GetJsonArrayAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(roles)));

        _delete.DeleteJsonAsync(Arg.Any<string>(), Arg.Any<List<JsonElement>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _sut.RemoveAllRealmRolesAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _delete.Received(1).DeleteJsonAsync(Arg.Any<string>(), Arg.Any<List<JsonElement>>(), Arg.Any<CancellationToken>());
    }
}