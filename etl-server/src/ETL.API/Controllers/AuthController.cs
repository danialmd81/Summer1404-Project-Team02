using System.Net.Http.Headers;
using System.Text.Json;
using ETL.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet("login")]
    public IActionResult Login(string redirectPath)
    {
        var authUrl = $"{_configuration["Authentication:Authority"]}/protocol/openid-connect/auth";
        var clientId = _configuration["Authentication:ClientId"];
        var redirectUri = $"{_configuration["Authentication:RedirectUri"]}/{redirectPath}";

        var finalUrl = $"{authUrl}?" +
                       $"client_id={clientId}&" +
                       $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                       $"response_type=code" +
                       $"&scope=openid&profile&email";

        return Ok(new { redirectUrl = finalUrl });
    }

    [HttpPost("callback")]
    public async Task<IActionResult> Callback([FromBody] CallbackRequest request)
    {
        if (string.IsNullOrEmpty(request.Code))
        {
            return BadRequest("Authorization code is missing.");
        }

        var tokenEndpoint = $"{_configuration["Authentication:Authority"]}/protocol/openid-connect/token";
        var clientId = _configuration["Authentication:ClientId"];
        var clientSecret = _configuration["Authentication:ClientSecret"];
        var redirectUri = $"{_configuration["Authentication:RedirectUri"]}/{request.RedirectPath}";

        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", request.Code },
            { "redirect_uri", redirectUri },
            { "client_id", clientId },
            { "client_secret", clientSecret }
        };

        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(requestBody));

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return BadRequest($"Token exchange failed: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<TokenResponse>(responseContent);

        if (tokenData == null || string.IsNullOrEmpty(tokenData.AccessToken))
        {
            return BadRequest("Access token not found in the response.");
        }

        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddSeconds(tokenData.AccessExpiresIn)
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddSeconds(tokenData.RefreshExpiresIn)
        };

        Response.Cookies.Append("access_token", tokenData.AccessToken, accessCookieOptions);
        Response.Cookies.Append("refresh_token", tokenData.RefreshToken, refreshCookieOptions);

        return Ok(new { message = "Authentication successful" });
    }


    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var accessToken = Request.Cookies["access_token"];
        var refreshToken = Request.Cookies["refresh_token"];

        var token = accessToken.StartsWith("Bearer ")
                ? accessToken.Substring(7)
                : accessToken;

        var logoutEndpoint = $"{_configuration["Authentication:Authority"]}/protocol/openid-connect/logout";
        var clientId = _configuration["Authentication:ClientId"];
        var clientSecret = _configuration["Authentication:ClientSecret"];

        var logoutData = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "refresh_token", refreshToken }
            };

        var httpClient = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, logoutEndpoint);
        request.Content = new FormUrlEncodedContent(logoutData);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return BadRequest($"Logout failed: {errorContent}");
        }

        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");

        Response.Headers.CacheControl = "no-cache, no-store";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.Expires = "0";

        return Ok(new { message = "Logout successful" });
    }
}