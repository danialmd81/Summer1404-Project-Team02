using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETL.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    
    // [HttpGet]
    // public ActionResult<string> GetRedirectUrl()
    // {
    //     
    // } 
    
    // [Authorize]
    // [HttpGet]
    // public Task<IActionResult> GetCurrentUserInfo()
    // {
    //     return  Task.FromResult<IActionResult>(new OkObjectResult("Hello, World!"));
    // }
    //
    // public async Task<IActionResult> LoginCallback()
    // {
    //     var authResult = await HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);
    //     if (authResult?.Succeeded != true)
    //     {
    //         // Handle failed authentication
    //         return RedirectToAction("Login");
    //     }
    //
    //     // Get the access token and refresh token
    //     var accessToken = authResult.Properties.GetTokenValue("access_token");
    //     var refreshToken = authResult.Properties.GetTokenValue("refresh_token");
    //
    //     // Set them as secure, HttpOnly cookies
    //     var cookieOptions = new CookieOptions
    //     {
    //         HttpOnly = true,     // prevent JavaScript from reading it
    //         Secure = true,       // only over HTTPS
    //         SameSite = SameSiteMode.Strict, // helps prevent CSRF
    //         Expires = DateTimeOffset.UtcNow.AddMinutes(30) // token lifetime
    //     };
    //
    //     HttpContext.Response.Cookies.Append("access_token", accessToken, cookieOptions);
    //     HttpContext.Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
    //     {
    //         HttpOnly = true,
    //         Secure = true,
    //         SameSite = SameSiteMode.Strict,
    //         Expires = DateTimeOffset.UtcNow.AddDays(7) // refresh lasts longer
    //     });
    //
    //     // Redirect the user to the desired page
    //     return RedirectToAction("Index");
    // }
    [HttpGet("public")]
    public IActionResult Public() => Ok("Anyone can see this");

    [Authorize]
    [HttpGet("secure")]
    public IActionResult Secure() => Ok($"Hello {User.Identity?.Name}");

    [Authorize(Roles = "user-admin")]
    [HttpGet("admin")]
    public IActionResult Admin() => Ok("You are an admin");
    
    // [HttpGet("/login")]
    // public IActionResult Login(string? returnUrl = "/secure")
    // {
    //     return Challenge(new AuthenticationProperties { RedirectUri = "/" },
    //         OpenIdConnectDefaults.AuthenticationScheme);
    //
    //    // return new AuthenticationProperties { RedirectUri = returnUrl };
    // }

    
    
}
