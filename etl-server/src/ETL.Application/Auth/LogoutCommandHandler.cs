using ETL.Application.Abstractions.Security;
using ETL.Application.Common;
using MediatR;

namespace ETL.Application.Auth;

public record LogoutCommand(string? AccessToken, string? RefreshToken) : IRequest<Result>;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IAuthLogoutService _logoutService;

    public LogoutCommandHandler(IAuthLogoutService logoutService)
    {
        _logoutService = logoutService ?? throw new ArgumentNullException(nameof(logoutService));
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var result = await _logoutService.LogoutAsync(request.AccessToken, request.RefreshToken, cancellationToken);

        if (result.IsFailure)
            return Result.Failure(result.Error);

        return Result.Success();
    }
}
