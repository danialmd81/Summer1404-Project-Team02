using System.Security.Claims;
using ETL.API.DTOs;
using ETL.Contracts.Security;

namespace ETL.Application.Abstractions;

public interface IAccountService
{
    UserProfileDto GetUserProfile(ClaimsPrincipal user);
    Task<bool> ChangePasswordAsync(ClaimsPrincipal user, ChangePasswordRequest request);
}