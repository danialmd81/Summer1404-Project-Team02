using ETL.Application.Abstractions.UserServices;
using ETL.Application.Common;
using MediatR;

namespace ETL.Application.User.ChangeRole
{
    public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, Result>
    {
        private readonly IOAuthUserRoleChanger _roleChanger;

        public ChangeUserRoleCommandHandler(IOAuthUserRoleChanger roleChanger)
        {
            _roleChanger = roleChanger ?? throw new ArgumentNullException(nameof(roleChanger));
        }

        public async Task<Result> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _roleChanger.ChangeRoleAsync(request.UserId, request.Role, cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                return Result.Failure(Error.Problem("User.ChangeRole.Failed", ex.Message));
            }
        }
    }
}
