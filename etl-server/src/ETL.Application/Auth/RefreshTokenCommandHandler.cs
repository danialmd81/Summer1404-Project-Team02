using ETL.Application.Abstractions.Security;
using ETL.Application.Common;
using ETL.Application.Common.DTOs;
using MediatR;

namespace ETL.Application.Auth;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<TokenResponse>>;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    private readonly IAuthTokenRefresher _tokenRefresher;

    public RefreshTokenCommandHandler(IAuthTokenRefresher tokenRefresher)
    {
        _tokenRefresher = tokenRefresher ?? throw new ArgumentNullException(nameof(tokenRefresher));
    }

    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Result.Failure<TokenResponse>(Error.Validation("Auth.Refresh.MissingToken", "Refresh token is required."));

        var refreshResult = await _tokenRefresher.RefreshAsync(request.RefreshToken, cancellationToken);

        if (refreshResult.IsFailure)
            return Result.Failure<TokenResponse>(refreshResult.Error);

        var tokens = refreshResult.Value;
        return Result.Success(tokens!);
    }
}
