using ETL.Application.Abstractions.Security;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.Security;

public class AuthLogoutService : IAuthLogoutService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthLogoutService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));;
    }

    public async Task LogoutAsync(string? accessToken, string? refreshToken, CancellationToken ct = default)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var logoutEndpoint = $"{_configuration["Authentication:Authority"]}/protocol/openid-connect/logout";
        var clientId = _configuration["Authentication:ClientId"];
        var clientSecret = _configuration["Authentication:ClientSecret"];

        var logoutData = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "refresh_token", refreshToken ?? string.Empty }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, logoutEndpoint);
        request.Content = new FormUrlEncodedContent(logoutData);

        if (!string.IsNullOrEmpty(accessToken))
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken.StartsWith("Bearer ") ? accessToken.Substring(7) : accessToken);

        var response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Logout failed: {err}");
        }
    }
}
