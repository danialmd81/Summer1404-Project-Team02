using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using ETL.Application.User.Delete;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ETL.Application.Tests.User;

public class DeleteUserCommandHandlerTests
{
    private readonly IOAuthUserDeleter _userDeleter;
    private readonly ILogger<DeleteUserCommandHandler> _logger;
    private readonly DeleteUserCommandHandler _sut;

    public DeleteUserCommandHandlerTests()
    {
        _userDeleter = Substitute.For<IOAuthUserDeleter>();
        _logger = Substitute.For<ILogger<DeleteUserCommandHandler>>();
        _sut = new DeleteUserCommandHandler(_userDeleter, _logger);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        var command = new DeleteUserCommand("");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.Delete.InvalidId");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDeleteSucceeds()
    {
        // Arrange
        var command = new DeleteUserCommand("user123");
        _userDeleter.DeleteUserAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDeleteFails()
    {
        // Arrange
        var command = new DeleteUserCommand("user123");
        var error = Error.Failure("OAuth.Failed", "something went wrong");
        _userDeleter.DeleteUserAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(error));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.Delete");
        result.Error.Description.Should().Contain("something went wrong");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenUserDeleterIsNull()
    {
        // Act
        Action act = () => new DeleteUserCommandHandler(null!, _logger);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("userDeleter");
    }

    [Fact]
    public void Constructor_ShouldNotThrow_WhenLoggerIsNull()
    {
        // Act
        Action act = () => new DeleteUserCommandHandler(_userDeleter, null!);

        // Assert
        act.Should().NotThrow();
    }
}