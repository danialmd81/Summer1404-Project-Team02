using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETL.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    [Authorize]
    [HttpGet]
    public Task<IActionResult> GetCurrentUserInfo()
    {
        return
    }
}
