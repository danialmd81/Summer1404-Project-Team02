using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using ETL.Infrastructure.UserServices;
using FluentAssertions;
using NSubstitute;

namespace ETL.Infrastructure.Tests.UserServices;

public class OAuthUserRoleChangerTests
{
    private readonly IRoleRemover _roleRemover;
    private readonly IOAuthRoleAssigner _roleAssigner;
    private readonly OAuthUserRoleChanger _sut;

    public OAuthUserRoleChangerTests()
    {
        _roleRemover = Substitute.For<IRoleRemover>();
        _roleAssigner = Substitute.For<IOAuthRoleAssigner>();

        _sut = new OAuthUserRoleChanger(_roleRemover, _roleAssigner);
    }

    // Constructor null-checks
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenRoleRemoverIsNull()
    {
        Action act = () => new OAuthUserRoleChanger(null!, _roleAssigner);
        act.Should().Throw<ArgumentNullException>().WithParameterName("roleRemover");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenRoleAssignerIsNull()
    {
        Action act = () => new OAuthUserRoleChanger(_roleRemover, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("roleAssigner");
    }

    [Fact]
    public async Task ChangeRoleAsync_ShouldReturnFailure_WhenRemoveAllRolesFails()
    {
        // Arrange
        var userId = "u1";
        var newRole = "role1";
        var error = Error.Problem("err", "remove failed");

        _roleRemover.RemoveAllRealmRolesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure(error)));

        // Act
        var result = await _sut.ChangeRoleAsync(userId, newRole);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        await _roleAssigner.DidNotReceive().AssignRoleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangeRoleAsync_ShouldReturnFailure_WhenAssignRoleFails()
    {
        // Arrange
        var userId = "u1";
        var newRole = "role1";
        var assignError = Error.Problem("assignErr", "assign failed");

        _roleRemover.RemoveAllRealmRolesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        _roleAssigner.AssignRoleAsync(userId, newRole, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure(assignError)));

        // Act
        var result = await _sut.ChangeRoleAsync(userId, newRole);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.ChangeRole.AssignFailed");
        result.Error.Description.Should().Contain("Failed to assign role 'role1'");
        result.Error.Description.Should().Contain(assignError.Code);
        result.Error.Description.Should().Contain(assignError.Description);
    }

    [Fact]
    public async Task ChangeRoleAsync_ShouldReturnSuccess_WhenRemoveAndAssignSucceed()
    {
        // Arrange
        var userId = "u1";
        var newRole = "role1";

        _roleRemover.RemoveAllRealmRolesAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        _roleAssigner.AssignRoleAsync(userId, newRole, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _sut.ChangeRoleAsync(userId, newRole);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}