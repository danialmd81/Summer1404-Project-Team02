using ETL.Application.Common;
using ETL.Application.Common.DTOs;
using MediatR;

namespace ETL.Application.User.GetAll
{
    public record GetAllUsersQuery(int? First = null, int? Max = null) : IRequest<Result<IEnumerable<UserDto>>>;
}
