using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using ETL.Application.Common.DTOs;
using ETL.Application.User;
using FluentAssertions;
using NSubstitute;

namespace ETL.Application.Tests.User;

public class GetAllUsersQueryHandlerTests
{
    private readonly IOAuthAllUserReader _allUserReader;
    private readonly IOAuthUserRoleGetter _roleGetter;
    private readonly GetAllUsersQueryHandler _sut;

    public GetAllUsersQueryHandlerTests()
    {
        _allUserReader = Substitute.For<IOAuthAllUserReader>();
        _roleGetter = Substitute.For<IOAuthUserRoleGetter>();
        _sut = new GetAllUsersQueryHandler(_allUserReader, _roleGetter);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAllUserReaderFails()
    {
        // Arrange
        var error = Error.Failure("OAuth.Users.Failed", "cannot fetch users");
        _allUserReader.GetAllAsync(0, 10, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<List<UserDto>>(error));

        var query = new GetAllUsersQuery(0, 10);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OAuth.Users.Failed");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoUsers()
    {
        // Arrange
        _allUserReader.GetAllAsync(0, 10, Arg.Any<CancellationToken>())
            .Returns(Result.Success(new List<UserDto>()));

        var query = new GetAllUsersQuery(0, 10);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRolesFetchedSuccessfully()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new UserDto { Id = "u1", Username = "user1", Email = "", FirstName = "", LastName = ""},
            new UserDto { Id = "u2", Username = "user2", Email = "", FirstName = "", LastName = "" }
        };

        _allUserReader.GetAllAsync(0, 10, Arg.Any<CancellationToken>())
            .Returns(Result.Success(users));

        _roleGetter.GetRoleForUserAsync("u1", Arg.Any<CancellationToken>())
            .Returns(Result.Success("Admin"));
        _roleGetter.GetRoleForUserAsync("u2", Arg.Any<CancellationToken>())
            .Returns(Result.Success("User"));

        var query = new GetAllUsersQuery(0, 10);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.First(u => u.Id == "u1").Role.Should().Be("Admin");
        result.Value.First(u => u.Id == "u2").Role.Should().Be("User");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAnyRoleFetchFails()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new UserDto { Id = "u1", Username = "user1", Email = "", FirstName = "", LastName = "" },
            new UserDto { Id = "u2", Username = "user2", Email = "", FirstName = "", LastName = "" }
        };

        _allUserReader.GetAllAsync(0, 10, Arg.Any<CancellationToken>())
            .Returns(Result.Success(users));

        _roleGetter.GetRoleForUserAsync("u1", Arg.Any<CancellationToken>())
            .Returns(Result.Success("Admin"));
        _roleGetter.GetRoleForUserAsync("u2", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string>(
                Error.Failure("Role.Fetch.Failed", "cannot fetch role")));

        var query = new GetAllUsersQuery(0, 10);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.GetAll.RoleFetchFailed");
        result.Error.Description.Should().Contain("u2");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAllUserReaderIsNull()
    {
        // Act
        Action act = () => new GetAllUsersQueryHandler(null!, _roleGetter);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("allUserReader");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRoleGetterIsNull()
    {
        // Act
        Action act = () => new GetAllUsersQueryHandler(_allUserReader, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("roleGetter");
    }
}