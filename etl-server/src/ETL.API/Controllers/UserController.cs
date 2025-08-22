using System.Security.Claims;
using ETL.API.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETL.API.Controllers;

public class UserProfileDto
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
}

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{

    [HttpGet("profile")]
    [Authorize]
    public IActionResult GetUserProfile()
    {
        var userProfile = new UserProfileDto
        {
            // Use ClaimTypes constants for the formal claim names
            Id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value, // <-- CHANGED
            Username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,         // <-- CHANGED
            Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,           // <-- CHANGED
            FirstName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value,     // <-- CHANGED
            LastName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value,       // <-- CHANGED

            // Your middleware correctly adds roles using ClaimTypes.Role,
            // so this will now work perfectly.
            Roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)         // <-- CHANGED
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