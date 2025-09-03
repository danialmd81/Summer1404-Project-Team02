using System.Security.Claims;
using ETL.Application.Abstractions.Security;
using ETL.Application.Auth.DTOs;
using ETL.Application.Common;
using MediatR;

namespace ETL.Application.Auth;

public record ChangePasswordCommand(ChangePasswordDto Request, ClaimsPrincipal User) : IRequest<Result>;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IAuthCredentialValidator _credentialValidator;
    private readonly IAuthRestPasswordService _restPasswordService;

    public ChangePasswordCommandHandler(IAuthCredentialValidator credentialValidator, IAuthRestPasswordService restPasswordService)
    {
        _credentialValidator = credentialValidator ?? throw new ArgumentNullException(nameof(credentialValidator));
        _restPasswordService = restPasswordService ?? throw new ArgumentNullException(nameof(restPasswordService));
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Request;
        if (dto is null)
            return Result.Failure(Error.Failure("Auth.InvalidRequest", "Request is missing"));

        if (dto.NewPassword != dto.ConfirmPassword)
            return Result.Failure(Error.Validation("Auth.PasswordMismatch", "New password and confirmation do not match"));

        var user = request.User;
        var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var username = user.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
            return Result.Failure(Error.Failure("Auth.UserNotFound", "User identity not found"));

        var valid = await _credentialValidator.ValidateCredentialsAsync(username, dto.CurrentPassword ?? string.Empty, cancellationToken);
        if (!valid)
            return Result.Failure(Error.Validation("Auth.InvalidCurrentPassword", "The current password is incorrect"));

        var result = await _restPasswordService.ResetPasswordAsync(userId, dto.NewPassword ?? string.Empty, cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        return Result.Success();
    }
}
