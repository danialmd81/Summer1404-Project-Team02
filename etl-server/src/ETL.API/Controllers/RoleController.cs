using ETL.Application.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETL.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class RoleController : ControllerBase
{
    [Authorize(Policy = Policy.CanReadRoles)]
    [HttpGet("all")]
    public IActionResult GetRoles()
    {
        return Ok(new { Role = Role.GetAllRoles() });
    }
}
