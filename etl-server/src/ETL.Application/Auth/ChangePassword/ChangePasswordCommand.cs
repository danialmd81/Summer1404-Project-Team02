using System.Security.Claims;
using ETL.Application.Auth.DTOs;
using ETL.Application.Common;
using MediatR;

namespace ETL.Application.Auth.ChangePassword;

public record ChangePasswordCommand(ChangePasswordDto Request, ClaimsPrincipal User) : IRequest<Result>;
