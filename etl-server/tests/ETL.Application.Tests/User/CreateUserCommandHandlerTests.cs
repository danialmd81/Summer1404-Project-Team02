using ETL.Application.Abstractions;
using ETL.Application.Common;
using ETL.Application.User.Create;
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
    public async Task Handle_ShouldReturnFailure_WhenUsernameOrPasswordIsEmpty()
    {
        var command = new CreateUserCommand("", "", "", "", "", 
            new List<string> {}); // invalid input

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("User.Create.InvalidInput");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserCreationFails()
    {
        var command = new CreateUserCommand("user", "pass", "", "", "", 
            new List<string> {});

        _userCreator.CreateUserAsync(command, default)
            .Returns(Result.Failure<string>(Error.Failure("User.Create.Failed", "duplicate")));

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("User.Create.Failed");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRoleAssignmentFails()
    {
        var command = new CreateUserCommand("user", "pass", "", "", "", 
            new[] { "Admin" });

        _userCreator.CreateUserAsync(command, default)
            .Returns(Result.Success("newUserId"));

        _roleAssigner.AssignRolesAsync("newUserId", command.Roles!, default)
            .Returns(Result.Failure(Error.Failure("Role.Assign.Failed", "No such role")));

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("User.Create.RoleAssignmentFailed");
        result.Error.Description.Should().Contain("newUserId");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserCreatedWithoutRoles()
    {
        var command = new CreateUserCommand("user", "pass", "", "", "", 
            new List<string> {});

        _userCreator.CreateUserAsync(command, default)
            .Returns(Result.Success("newUserId"));

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("newUserId");

        await _roleAssigner.DidNotReceiveWithAnyArgs()
            .AssignRolesAsync(default!, default!, default);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserCreatedWithRoles()
    {
        var command = new CreateUserCommand("user", "pass", "", "", "", 
            new[] { "Admin" });

        _userCreator.CreateUserAsync(command, default)
            .Returns(Result.Success("newUserId"));

        _roleAssigner.AssignRolesAsync("newUserId", command.Roles!, default)
            .Returns(Result.Success());

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("newUserId");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenExceptionThrown()
    {
        var command = new CreateUserCommand("user", "pass", "", "", "", 
            new List<string> {});

        _userCreator.CreateUserAsync(command, default)
            .Throws(new Exception("DB connection failed"));

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("User.Create.Failed");
        result.Error.Description.Should().Contain("DB connection failed");
    }
}