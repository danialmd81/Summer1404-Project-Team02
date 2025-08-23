using System.Security.Claims;
using ETL.API.DTOs;
using ETL.API.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    [Authorize]
    [HttpGet("profile")]
    public IActionResult GetUserProfile()
    {
        var userProfile = new UserProfileDto
        {
            Id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
            Username = User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value,
            Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
            FirstName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value,
            LastName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value,
            Roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
        };

        return Ok(userProfile);
    }

    [Authorize(Policy = Policies.SystemAdminOnly)]
    [HttpGet("admin")]
    public IActionResult Admin()
    {
        var user = HttpContext.User;
        return Ok(User.IsInRole(Roles.SystemAdmin));
    }

}