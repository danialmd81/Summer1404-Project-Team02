using ETL.Application.Common;
using MediatR;

namespace ETL.Application.User.Create;

public record CreateUserCommand(
    string Username,
    string Password,
    string? Email,
    string? FirstName,
    string? LastName,
    IEnumerable<string>? Roles
) : IRequest<Result<string>>;
