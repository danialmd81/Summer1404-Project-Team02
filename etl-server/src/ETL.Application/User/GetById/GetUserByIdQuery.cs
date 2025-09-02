using ETL.Application.Common;
using ETL.Application.Common.DTOs;
using MediatR;

namespace ETL.Application.User.GetById;

public record GetUserByIdQuery(string UserId) : IRequest<Result<UserDto>>;
