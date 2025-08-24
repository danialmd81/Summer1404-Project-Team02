using ETL.Application.Common;
using MediatR;

namespace ETL.Application.Auth.Logout;

public record LogoutCommand(string? AccessToken, string? RefreshToken) : IRequest<Result>;
