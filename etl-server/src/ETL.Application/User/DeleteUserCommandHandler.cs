using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ETL.Application.User.Delete;

public record DeleteUserCommand(string UserId) : IRequest<Result>;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IOAuthUserDeleter _userDeleter;

    public DeleteUserCommandHandler(IOAuthUserDeleter userDeleter, ILogger<DeleteUserCommandHandler> logger)
    {
        _userDeleter = userDeleter ?? throw new ArgumentNullException(nameof(userDeleter));
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return Result.Failure(Error.Failure("User.Delete.InvalidId", "User id is required"));

        var result = await _userDeleter.DeleteUserAsync(request.UserId, cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure(Error.Failure("User.Delete", $"Failed to delete user {request.UserId} via OAuth: {result.Error.Code} - {result.Error.Description}"));
        }

        return Result.Success();
    }
}