using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using ETL.Application.User;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ETL.Application.Tests.User;

public class ChangeUserRoleCommandHandlerTests
{
    private readonly IOAuthUserRoleChanger _roleChanger;
    private readonly ChangeUserRoleCommandHandler _sut;

    public ChangeUserRoleCommandHandlerTests()
    {
        _roleChanger = Substitute.For<IOAuthUserRoleChanger>();
        _sut = new ChangeUserRoleCommandHandler(_roleChanger);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRoleChangeSucceeds()
    {
        // Arrange
        var command = new ChangeUserRoleCommand("user123", "Admin");
        _roleChanger.ChangeRoleAsync(command.UserId, command.Role, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRoleChangeFails()
    {
        // Arrange
        var command = new ChangeUserRoleCommand("user123", "Admin");
        var error = Error.Failure("Role.Failed", "Unable to change role");
        _roleChanger.ChangeRoleAsync(command.UserId, command.Role, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(error));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenExceptionIsThrown()
    {
        // Arrange
        var command = new ChangeUserRoleCommand("user123", "Admin");
        _roleChanger.ChangeRoleAsync(command.UserId, command.Role, Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("something went wrong"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.ChangeRole.Failed");
        result.Error.Description.Should().Contain("something went wrong");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRoleChangerIsNull()
    {
        // Act
        Action act = () => new ChangeUserRoleCommandHandler(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("roleChanger");
    }
}