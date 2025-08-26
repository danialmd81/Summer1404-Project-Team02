using ETL.Application.Common;
using MediatR;

namespace ETL.Application.User.Create;

public record CreateUserCommand(
    string Username,
    string? Email,
    string? FirstName,
    string? LastName,
    string Password,
    string? Role
) : IRequest<Result<string>>;
