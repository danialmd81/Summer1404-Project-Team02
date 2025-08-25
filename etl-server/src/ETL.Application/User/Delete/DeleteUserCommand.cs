using ETL.Application.Common;
using MediatR;

namespace ETL.Application.User.Delete;

public record DeleteUserCommand(string UserId) : IRequest<Result>;
