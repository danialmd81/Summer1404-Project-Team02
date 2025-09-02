using System.Net.Http.Json;
using ETL.Application.Abstractions.Security;
using ETL.Application.Common;
using ETL.Application.Common.DTOs;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.Security;

public class AuthCodeForTokenExchanger : IAuthCodeForTokenExchanger
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthCodeForTokenExchanger(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration)); ;
    }

    public async Task<Result<TokenResponse>> ExchangeCodeForTokensAsync(string code, string redirectPath, CancellationToken ct = default)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var tokenEndpoint = $"{_configuration["Authentication:Authority"]}/protocol/openid-connect/token";
        var clientId = _configuration["Authentication:ClientId"];
        var clientSecret = _configuration["Authentication:ClientSecret"];
        var redirectUri = $"{_configuration["Authentication:RedirectUri"]}/{redirectPath}";

        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectUri },
            { "client_id", clientId },
            { "client_secret", clientSecret }
        };

        var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(requestBody), ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            return Result.Failure<TokenResponse>(Error.Failure("Auth.TokenExchangeFailed", $"Token exchange failed: {err}"));
        }

        var tokenData = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
        return tokenData;
    }
}
