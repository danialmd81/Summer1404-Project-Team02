using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ETL.API.DTOs;
using ETL.API.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ISsoAdminService _keycloakAdminService;


    public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ISsoAdminService keycloakAdminService)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _keycloakAdminService = keycloakAdminService;
    }

    [HttpPost("change-password")]
    [Authorize] 
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmPassword)
        {
            return BadRequest("New password and confirmation password do not match.");
        }

        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var username = User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
        {
            return Unauthorized("User identity not found.");
        }

        var httpClient = _httpClientFactory.CreateClient();
        var authority = _configuration["Authentication:Authority"];
        var tokenEndpoint = $"{authority}/protocol/openid-connect/token";

        // --- Step 1: Verify the user's CURRENT password ---
        var passwordVerificationBody = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _configuration["Authentication:ClientId"], // Uses the user-facing client
            ["client_secret"] = _configuration["Authentication:ClientSecret"],
            ["username"] = username,
            ["password"] = request.CurrentPassword
        };
        
        var passwordResponse = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(passwordVerificationBody));
        if (!passwordResponse.IsSuccessStatusCode)
        {
            return BadRequest("The current password is incorrect.");
        }

        // --- Step 2: Get an admin token for the backend service ---
        var adminAccessToken = await _keycloakAdminService.GetAdminAccessTokenAsync();
        if (string.IsNullOrEmpty(adminAccessToken))
        {
            return StatusCode(500, "Could not obtain admin credentials.");
        }
        
        // --- Step 3: Call the Admin API to set the NEW password ---
             
        // 👇 THIS IS THE CORRECTED PART
        var keycloakBaseUrl = _configuration["Authentication:KeycloakBaseUrl"];
        var realm = _configuration["Authentication:Realm"];
        
        // This now constructs the correct URL, avoiding the 404 error.
        var resetPasswordUrl = $"{keycloakBaseUrl}/admin/realms/{realm}/users/{userId}/reset-password";
        
        var resetPasswordPayload = new { type = "password", temporary = false, value = request.NewPassword };
        
        var jsonContent = new StringContent(JsonSerializer.Serialize(resetPasswordPayload), Encoding.UTF8, "application/json");
        var resetRequest = new HttpRequestMessage(HttpMethod.Put, resetPasswordUrl);
        resetRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken); // Assumes adminAccessToken is fetched
        resetRequest.Content = jsonContent;

        var resetResponse = await httpClient.SendAsync(resetRequest);
        if (!resetResponse.IsSuccessStatusCode)
        {
            return StatusCode(500, "Failed to update password in Keycloak.");
        }

        return Ok(new { message = "Password changed successfully." });
    }
}