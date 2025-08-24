using System.Net.Http.Json;
using ETL.Application.Abstractions.Security;
using ETL.Application.Common.DTOs;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.Security;

public class AdminTokenService : IAdminTokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AdminTokenService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<string?> GetAdminAccessTokenAsync(CancellationToken ct = default)
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

        var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(adminTokenBody), ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Failed to obtain admin access token from OAuth. {err}");
        }

        var adminTokenData = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
        return adminTokenData?.AccessToken;
    }
}
