using ETL.Application.Common;
using ETL.Application.Common.DTOs;
using MediatR;

namespace ETL.Application.Auth.Refresh;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<TokenResponse>>;
