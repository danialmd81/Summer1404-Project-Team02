namespace ETL.Application.User.GetCurrent;
using System.Security.Claims;
using ETL.Application.Common.DTOs;
using MediatR;

public record GetUserProfileQuery(ClaimsPrincipal User) : IRequest<UserDto>;
