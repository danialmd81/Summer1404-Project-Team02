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
        var result = await _exchanger.ExchangeCodeForTokensAsync(request.Code, request.RedirectPath ?? string.Empty, cancellationToken);

        if (result.IsFailure)
            return Result.Failure<TokenResponse>(result.Error);

        return Result.Success(result.Value);
    }
}
