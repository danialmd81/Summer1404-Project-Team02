using ETL.Application.Abstractions.Security;
using ETL.Application.Auth.Logout;
using ETL.Application.Common;
using FluentAssertions;
using NSubstitute;

namespace ETL.Application.Tests.Auth
{
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

            _logoutService
                .LogoutAsync("access", "refresh", Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Success()));

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            await _logoutService.Received(1)
                .LogoutAsync("access", "refresh", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenLogoutReturnsFailure()
        {
            var command = new LogoutCommand("access", "refresh");

            var error = Error.Failure("Auth.LogoutFailed", "Service unavailable");
            _logoutService
                .LogoutAsync("access", "refresh", Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Failure(error)));

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("Auth.LogoutFailed");
            result.Error.Description.Should().Contain("Service unavailable");
        }
    }
}
