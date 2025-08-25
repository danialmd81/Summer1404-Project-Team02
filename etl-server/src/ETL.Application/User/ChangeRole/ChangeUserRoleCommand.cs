using ETL.Application.Common;
using MediatR;

namespace ETL.Application.User.ChangeRole;
public record ChangeUserRoleCommand(string UserId, string Role) : IRequest<Result>;
