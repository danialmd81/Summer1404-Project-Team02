using ETL.Application.Abstractions;
using ETL.Application.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ETL.Application.User.Delete
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
    {
        private readonly IOAuthUserDeleter _userDeleter;
        private readonly ILogger<DeleteUserCommandHandler> _logger;

        public DeleteUserCommandHandler(IOAuthUserDeleter userDeleter, ILogger<DeleteUserCommandHandler> logger)
        {
            _userDeleter = userDeleter;
            _logger = logger;
        }

        public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                return Result.Failure(Error.Failure("User.Delete.InvalidId", "User id is required"));

            var result = await _userDeleter.DeleteUserAsync(request.UserId, cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogWarning("Failed to delete user {UserId} via OAuth: {ErrorCode} - {ErrorDesc}", request.UserId, result.Error.Code, result.Error.Description);
                return result;
            }

            return Result.Success();
        }
    }
}
