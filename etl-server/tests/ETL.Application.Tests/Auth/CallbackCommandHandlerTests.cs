using ETL.Application.Abstractions.Security;
using ETL.Application.Auth.LoginCallback;
using ETL.Application.Common.DTOs;
using FluentAssertions;
using NSubstitute;

namespace ETL.Application.Tests.Auth;

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
    public async Task Handle_ShouldReturnFailure_WhenTokensAreNull()
    {
        var command = new LoginCallbackCommand("authcode", "redirect");

        _exchanger.ExchangeCodeForTokensAsync("authcode", "redirect", default)
            .Returns((TokenResponse?)null);

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Auth.TokenExchangeFailed");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAccessTokenIsEmpty()
    {
        var command = new LoginCallbackCommand("authcode", "redirect");

        var tokens = new TokenResponse
        {
            AccessToken = "",
            RefreshToken = "ref",
            AccessExpiresIn = 3600
        };

        _exchanger.ExchangeCodeForTokensAsync("authcode", "redirect", default)
            .Returns(tokens);

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Auth.TokenExchangeFailed");
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

        _exchanger.ExchangeCodeForTokensAsync("authcode", "redirect", default)
            .Returns(tokens);

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(tokens);
    }

    [Fact]
    public async Task Handle_ShouldPassEmptyRedirectPath_WhenNullProvided()
    {
        var command = new LoginCallbackCommand("authcode", null);

        var tokens = new TokenResponse { AccessToken = "valid" };

        _exchanger.ExchangeCodeForTokensAsync("authcode", "", default)
            .Returns(tokens);

        var result = await _sut.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        await _exchanger.Received(1)
            .ExchangeCodeForTokensAsync("authcode", "", default);
    }
}