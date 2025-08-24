using System.Security.Claims;
using ETL.Application.Abstractions.Security;
using ETL.Application.Common;
using MediatR;

namespace ETL.Application.Auth.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IAuthCredentialValidator _credentialValidator;
    private readonly IAuthRestPasswordService _restPasswordService;

    public ChangePasswordCommandHandler(IAuthCredentialValidator credentialValidator, IAuthRestPasswordService restPasswordService)
    {
        _credentialValidator = credentialValidator;
        _restPasswordService = restPasswordService;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Request;
        if (dto is null)
            return Result.Failure(Error.Failure("Auth.InvalidRequest", "Request is missing"));

        if (dto.NewPassword != dto.ConfirmPassword)
            return Result.Failure(Error.Failure("Auth.PasswordMismatch", "New password and confirmation do not match"));

        var user = request.User;
        var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var username = user.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
            return Result.Failure(Error.Failure("Auth.UserNotFound", "User identity not found"));

        var valid = await _credentialValidator.ValidateCredentialsAsync(username, dto.CurrentPassword ?? string.Empty, cancellationToken);
        if (!valid)
            return Result.Failure(Error.Failure("Auth.InvalidCurrentPassword", "The current password is incorrect"));

        try
        {
            await _restPasswordService.ResetPasswordAsync(userId, dto.NewPassword ?? string.Empty, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Problem("Auth.ResetFailed", $"Failed to reset password: {ex.Message}"));
        }
    }
}
