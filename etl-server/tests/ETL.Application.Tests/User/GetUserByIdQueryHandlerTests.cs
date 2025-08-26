using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using ETL.Application.Common.DTOs;
using ETL.Application.User.GetById;
using FluentAssertions;
using NSubstitute;

namespace ETL.Application.Tests.User;

public class GetUserByIdQueryHandlerTests
{
    private readonly IOAuthUserReader _userReader;
    private readonly IOAuthUserRoleGetter _roleGetter;
    private readonly GetUserByIdQueryHandler _sut;

    public GetUserByIdQueryHandlerTests()
    {
        _userReader = Substitute.For<IOAuthUserReader>();
        _roleGetter = Substitute.For<IOAuthUserRoleGetter>();
        _sut = new GetUserByIdQueryHandler(_userReader, _roleGetter);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserIdIsNull()
    {
        // Arrange
        var query = new GetUserByIdQuery(null!);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidId");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_UserNotFound()
    {
        // Arrange
        var query = new GetUserByIdQuery("u1");
        _userReader.GetByIdAsync("u1", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UserDto>(Error.NotFound("User.NotFound", "User not found")));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.NotFound");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_RoleFetchFails()
    {
        // Arrange
        var query = new GetUserByIdQuery("u1");
        var user = new UserDto { Id = "u1", Username = "test", FirstName = "",  LastName = "", Email = "" };
        _userReader.GetByIdAsync("u1", Arg.Any<CancellationToken>())
            .Returns(Result.Success<UserDto>(user));

        _roleGetter.GetRoleForUserAsync("u1", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string>(Error.Problem("Role.Fetch.Failed", "could not fetch")));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Role.Fetch.Failed");
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WithUserAndRole()
    {
        // Arrange
        var query = new GetUserByIdQuery("u1");
        var user = new UserDto { Id = "u1", Username = "test", FirstName = "",  LastName = "", Email = ""};
        _userReader.GetByIdAsync("u1", Arg.Any<CancellationToken>())
            .Returns(Result.Success<UserDto>(user));

        _roleGetter.GetRoleForUserAsync("u1", Arg.Any<CancellationToken>())
            .Returns(Result.Success<string>("Admin"));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be("Admin");
    }

    [Fact]
    public void Constructor_Should_Throw_When_UserReaderIsNull()
    {
        // Act
        Action act = () => new GetUserByIdQueryHandler(null!, _roleGetter);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("userReader");
    }

    [Fact]
    public void Constructor_Should_Throw_When_RoleGetterIsNull()
    {
        // Act
        Action act = () => new GetUserByIdQueryHandler(_userReader, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("roleGetter");
    }
}