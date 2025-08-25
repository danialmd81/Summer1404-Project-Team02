using ETL.Application.Common;
using MediatR;

namespace ETL.Application.User.Edit;

public record EditUserCommand(
    string UserId,
    string? Username,
    string? Email,
    string? FirstName,
    string? LastName
) : IRequest<Result>;
