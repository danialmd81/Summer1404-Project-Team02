using ETL.Application.Abstractions.Security;
using ETL.Application.Auth.LoginCallback;
using ETL.Application.Common;
using ETL.Application.Common.DTOs;
using FluentAssertions;
using NSubstitute;

namespace ETL.Application.Tests.Auth
{
    public class CallbackCommandHandlerTests
    {
        private readonly IAuthCodeForTokenExchanger _exchanger;
        private readonly CallbackCommandHandler _sut;

        public CallbackCommandHandlerTests()
        {
            _exchanger = Substitute.For<IAuthCodeForTokenExchanger>();
            _sut = new CallbackCommandHandler(_exchanger);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenExchangerReturnsFailure()
        {
            var command = new LoginCallbackCommand("authcode", "redirect");

            var error = Error.Failure("Auth.TokenExchangeFailed", "Token exchange failed");
            var failureResult = Result.Failure<TokenResponse>(error);

            _exchanger
                .ExchangeCodeForTokensAsync("authcode", "redirect", Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(failureResult));

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("Auth.TokenExchangeFailed");
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenExchangerReturnsSuccess_EvenIfAccessTokenIsEmpty()
        {
            var command = new LoginCallbackCommand("authcode", "redirect");

            var tokens = new TokenResponse
            {
                AccessToken = string.Empty,
                RefreshToken = "ref",
                AccessExpiresIn = 3600
            };

            _exchanger
                .ExchangeCodeForTokensAsync("authcode", "redirect", Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Success(tokens)));

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(tokens);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenTokensAreValid()
        {
            var command = new LoginCallbackCommand("authcode", "redirect");

            var tokens = new TokenResponse
            {
                AccessToken = "valid_token",
                RefreshToken = "ref",
                AccessExpiresIn = 3600
            };

            _exchanger
                .ExchangeCodeForTokensAsync("authcode", "redirect", Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Success(tokens)));

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(tokens);
        }

        [Fact]
        public async Task Handle_ShouldPassEmptyRedirectPath_WhenNullProvided()
        {
            var command = new LoginCallbackCommand("authcode", null);

            var tokens = new TokenResponse { AccessToken = "valid" };

            _exchanger
                .ExchangeCodeForTokensAsync("authcode", "", Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Success(tokens)));

            var result = await _sut.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            await _exchanger.Received(1)
                .ExchangeCodeForTokensAsync("authcode", "", Arg.Any<CancellationToken>());
        }
    }
}
