using ETL.Application.Auth.ChangePassword;
using ETL.Application.Auth.DTOs;
using ETL.Application.Auth.LoginCallback;
using ETL.Application.Auth.Logout;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public AuthController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    [HttpGet("login")]
    public IActionResult Login([FromQuery] string redirectPath)
    {
        var authUrl = $"{_configuration["Authentication:Authority"]}/protocol/openid-connect/auth";
        var clientId = _configuration["Authentication:ClientId"];
        var redirectUri = $"{_configuration["Authentication:RedirectUri"]}/{redirectPath}";

        var finalUrl = $"{authUrl}?" +
                       $"client_id={Uri.EscapeDataString(clientId)}&" +
                       $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                       $"response_type=code&" +
                       $"scope=openid%20profile%20email";

        return Ok(new { redirectUrl = finalUrl });
    }

    [HttpPost("login-callback")]
    public async Task<IActionResult> Callback([FromBody] LoginCallbackCommand request)
    {
        var result = await _mediator.Send(request);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Code, message = result.Error.Description });

        var tokens = result.Value;

        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddSeconds(tokens.AccessExpiresIn)
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddSeconds(tokens.RefreshExpiresIn)
        };

        Response.Cookies.Append("access_token", tokens.AccessToken ?? string.Empty, accessCookieOptions);
        Response.Cookies.Append("refresh_token", tokens.RefreshToken ?? string.Empty, refreshCookieOptions);

        return Ok(new { message = "Authentication successful" });
    }

    [Authorize]
    [HttpPost("change-pass")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        var result = await _mediator.Send(new ChangePasswordCommand(request, User));

        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Code, message = result.Error.Description });

        return Ok(new { message = "Password changed successfully." });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var accessToken = Request.Cookies["access_token"];
        var refreshToken = Request.Cookies["refresh_token"];

        var result = await _mediator.Send(new LogoutCommand(accessToken, refreshToken));

        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Code, message = result.Error.Description });

        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");

        Response.Headers.CacheControl = "no-cache, no-store";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.Expires = "0";

        return Ok(new { message = "Logout successful" });
    }
}
