using System.Security.Claims;
using ETL.Application.Common.DTOs;
using MediatR;

namespace ETL.Application.User.GetCurrent;

public record GetUserProfileQuery(ClaimsPrincipal User) : IRequest<UserDto>;
