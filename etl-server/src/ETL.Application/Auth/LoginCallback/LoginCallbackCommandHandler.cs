using ETL.Application.Abstractions.Security;
using ETL.Application.Common;
using ETL.Application.Common.DTOs;
using MediatR;

namespace ETL.Application.Auth.LoginCallback;

public class CallbackCommandHandler : IRequestHandler<LoginCallbackCommand, Result<TokenResponse>>
{
    private readonly IAuthCodeForTokenExchanger _exchanger;

    public CallbackCommandHandler(IAuthCodeForTokenExchanger exchanger)
    {
        _exchanger = exchanger ?? throw new ArgumentNullException(nameof(exchanger));
    }

    public async Task<Result<TokenResponse>> Handle(LoginCallbackCommand request, CancellationToken cancellationToken)
    {
        var tokens = await _exchanger.ExchangeCodeForTokensAsync(request.Code, request.RedirectPath ?? string.Empty, cancellationToken);

        if (tokens is null || string.IsNullOrEmpty(tokens.AccessToken))
            return Result.Failure<TokenResponse>(Error.Failure("Auth.TokenExchangeFailed", "Token exchange failed"));

        return Result.Success(tokens);
    }
}
