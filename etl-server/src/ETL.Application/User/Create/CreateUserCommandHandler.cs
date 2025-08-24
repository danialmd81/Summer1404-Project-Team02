using ETL.Application.Abstractions;
using ETL.Application.Common;
using MediatR;

namespace ETL.Application.User.Create
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<string>>
    {
        private readonly IOAuthUserCreator _userCreator;
        private readonly IOAuthRoleAssigner _roleAssigner;

        public CreateUserCommandHandler(IOAuthUserCreator userCreator, IOAuthRoleAssigner roleAssigner)
        {
            _userCreator = userCreator;
            _roleAssigner = roleAssigner;
        }

        public async Task<Result<string>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return Result.Failure<string>(Error.Failure("User.Create.InvalidInput", "Username and password are required"));

            try
            {
                var createResult = await _userCreator.CreateUserAsync(request, cancellationToken);
                if (createResult.IsFailure)
                    return Result.Failure<string>(createResult.Error);

                var newUserId = createResult.Value;

                if (request.Roles is { } roles && roles.Any())
                {
                    var assignResult = await _roleAssigner.AssignRolesAsync(newUserId, roles, cancellationToken);
                    if (assignResult.IsFailure)
                    {
                        // Optional: try delete user here if you want to rollback
                        return Result.Failure<string>(Error.Problem("User.Create.RoleAssignmentFailed", $"User created (id={newUserId}) but role assignment failed: {assignResult.Error.Description}"));
                    }
                }

                return Result.Success(newUserId);
            }
            catch (Exception ex)
            {
                return Result.Failure<string>(Error.Problem("User.Create.Failed", ex.Message));
            }
        }
    }
}
