using ETL.Application.Abstractions.Security;
using ETL.Application.Auth.Logout;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ETL.Application.Tests.Auth;

public class LogoutCommandHandlerTests
{
    private readonly IAuthLogoutService _logoutService;
    private readonly LogoutCommandHandler _sut;

    public LogoutCommandHandlerTests()
    {
        _logoutService = Substitute.For<IAuthLogoutService>();
        _sut = new LogoutCommandHandler(_logoutService);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenLogoutSucceeds()
    {
        var command = new LogoutCommand("access", "refresh");

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        await _logoutService.Received(1)
            .LogoutAsync("access", "refresh", default);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenLogoutThrows()
    {
        var command = new LogoutCommand("access", "refresh");

        _logoutService.LogoutAsync("access", "refresh", default)
            .Throws(new Exception("Service unavailable"));

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Auth.LogoutFailed");
        result.Error.Description.Should().Contain("Service unavailable");
    }
}