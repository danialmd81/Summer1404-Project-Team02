using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using ETL.Application.User;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ETL.Application.Tests.User;

public class CreateUserCommandHandlerTests
{
    private readonly IOAuthUserCreator _userCreator;
    private readonly IOAuthRoleAssigner _roleAssigner;
    private readonly CreateUserCommandHandler _sut;

    public CreateUserCommandHandlerTests()
    {
        _userCreator = Substitute.For<IOAuthUserCreator>();
        _roleAssigner = Substitute.For<IOAuthRoleAssigner>();
        _sut = new CreateUserCommandHandler(_userCreator, _roleAssigner);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUsernameOrPasswordIsMissing()
    {
        // Arrange
        var command = new CreateUserCommand("", "", "", "", "", "");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.Create.InvalidInput");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserCreationFails()
    {
        // Arrange
        var command = new CreateUserCommand("testuser", "", "", "", 
            "password", "");
        _userCreator.CreateUserAsync(command, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string>(Error.Problem("User.Create.Failed", "creation error")));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.Create.Failed");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRoleAssignmentFails()
    {
        // Arrange
        var command = new CreateUserCommand("testuser", "", "", "", 
            "password", "admin");
        _userCreator.CreateUserAsync(command, Arg.Any<CancellationToken>())
            .Returns(Result.Success("new-user-id"));
        _roleAssigner.AssignRoleAsync("new-user-id", "admin", Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Failure("Role.Error", "role assignment failed")));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.Create.RoleAssignmentFailed");
        result.Error.Description.Should().Contain("role assignment failed");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserCreatedWithoutRole()
    {
        // Arrange
        var command = new CreateUserCommand("testuser", "", "", "", 
            "password", "");
        _userCreator.CreateUserAsync(command, Arg.Any<CancellationToken>())
            .Returns(Result.Success("new-user-id"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("new-user-id");
        await _roleAssigner.DidNotReceive()
            .AssignRoleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserCreatedAndRoleAssigned()
    {
        // Arrange
        var command = new CreateUserCommand("testuser", "", "", "", 
            "password", "admin");
        _userCreator.CreateUserAsync(command, Arg.Any<CancellationToken>())
            .Returns(Result.Success("new-user-id"));
        _roleAssigner.AssignRoleAsync("new-user-id", "admin", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("new-user-id");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenExceptionIsThrown()
    {
        // Arrange
        var command = new CreateUserCommand("testuser", "", "", "", 
            "password", "");
        _userCreator.CreateUserAsync(command, Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("unexpected error"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.Create.Failed");
        result.Error.Description.Should().Contain("unexpected error");
    }
    [Fact]
    public void Constructor_ShouldThrow_WhenUserCreatorIsNull()
    {
        // Act
        Action act = () => new CreateUserCommandHandler(null!, Substitute.For<IOAuthRoleAssigner>());

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("userCreator");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRoleAssignerIsNull()
    {
        // Act
        Action act = () => new CreateUserCommandHandler(Substitute.For<IOAuthUserCreator>(), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("roleAssigner");
    }
}