using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ETL.API.DTOs;

namespace ETL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SystemAdminOnly")]
public class AdminController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AdminController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }


    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var keycloakBaseUrl = _configuration["Authentication:KeycloakBaseUrl"];
        var realm = _configuration["Authentication:Realm"];

        // --- Step 1: Get an admin token for the backend service ---
        var adminAccessToken = await GetAdminAccessToken(httpClient);
        if (string.IsNullOrEmpty(adminAccessToken))
        {
            return StatusCode(500, "Could not obtain admin credentials.");
        }

        // --- Step 2: Create the user in Keycloak ---
        var createUserUrl = $"{keycloakBaseUrl}/admin/realms/{realm}/users";
        var newUserPayload = new
        {
            username = request.Username,
            email = request.Email,
            firstName = request.FirstName,
            lastName = request.LastName,
            enabled = true,
            credentials = new[]
            {
                new { type = "password", value = request.Password, temporary = false }
            }
        };

        var createRequest = new HttpRequestMessage(HttpMethod.Post, createUserUrl);
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        createRequest.Content = new StringContent(JsonSerializer.Serialize(newUserPayload), Encoding.UTF8, "application/json");
        var createResponse = await httpClient.SendAsync(createRequest);

        if (!createResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)createResponse.StatusCode, $"Failed to create user: {await createResponse.Content.ReadAsStringAsync()}");
        }

        // --- Step 3: Get the ID of the newly created user ---
        var newUserLocation = createResponse.Headers.Location;
        if (newUserLocation == null)
        {
            return StatusCode(500, "Keycloak did not return user location header.");
        }
        var newUserId = newUserLocation.ToString().Split('/').Last();

        // --- Step 4: Get role representations from Keycloak ---
        var rolesToAssign = new List<object>();
        foreach (var roleName in request.Roles ?? Enumerable.Empty<string>())
        {
            var getRoleUrl = $"{keycloakBaseUrl}/admin/realms/{realm}/roles/{roleName}";
            var getRoleRequest = new HttpRequestMessage(HttpMethod.Get, getRoleUrl);
            getRoleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
            var roleResponse = await httpClient.SendAsync(getRoleRequest);

            if (roleResponse.IsSuccessStatusCode)
            {
                var roleRepresentation = await roleResponse.Content.ReadFromJsonAsync<object>();
                rolesToAssign.Add(roleRepresentation);
            }
        }

        // --- Step 5: Assign the roles to the new user ---
        if (rolesToAssign.Any())
        {
            var assignRoleUrl = $"{keycloakBaseUrl}/admin/realms/{realm}/users/{newUserId}/role-mappings/realm";

            var assignRoleRequest = new HttpRequestMessage(HttpMethod.Post, assignRoleUrl);
            assignRoleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
            assignRoleRequest.Content = new StringContent(JsonSerializer.Serialize(rolesToAssign), Encoding.UTF8, "application/json");

            var assignRoleResponse = await httpClient.SendAsync(assignRoleRequest);
            if (!assignRoleResponse.IsSuccessStatusCode)
            {
                return StatusCode(500, $"User created, but failed to assign roles: {await assignRoleResponse.Content.ReadAsStringAsync()}");
            }
        }

        return StatusCode(201, new { message = $"User '{request.Username}' created successfully." });
    }


    private async Task<string?> GetAdminAccessToken(HttpClient httpClient)
    {
        var tokenEndpoint = $"{_configuration["Authentication:Authority"]}/protocol/openid-connect/token";
        var adminClientId = _configuration["KeycloakAdmin:ClientId"];
        var adminClientSecret = _configuration["KeycloakAdmin:ClientSecret"];

        var adminTokenBody = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = adminClientId,
            ["client_secret"] = adminClientSecret
        };

        var adminTokenResponse = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(adminTokenBody));
        if (!adminTokenResponse.IsSuccessStatusCode) return null;

        var adminTokenData = await adminTokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        return adminTokenData?.AccessToken;
    }
}


// DTOs for the Create User flow. You can place these in a separate DTOs folder.
public class CreateUserRequest
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<string>? Roles { get; set; }
}
