using System.Text.Json;
using ETL.API.DTOs;
using ETL.API.Services.Abstraction;

namespace ETL.API.Services;

public class KeycloakAdminService : ISsoAdminService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public KeycloakAdminService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<string?> GetAdminAccessTokenAsync()
    {
        var httpClient = _httpClientFactory.CreateClient();
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
        
        if (!adminTokenResponse.IsSuccessStatusCode)
        {
            // In a real application, you should add logging here.
            return null;
        }

        var adminTokenData = await adminTokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        return adminTokenData?.AccessToken;
    }
}