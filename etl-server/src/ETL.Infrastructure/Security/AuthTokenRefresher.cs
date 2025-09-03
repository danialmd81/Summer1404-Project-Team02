using System.Net.Http.Json;
using ETL.Application.Abstractions.Security;
using ETL.Application.Common;
using ETL.Application.Common.DTOs;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.Security;

public class AuthTokenRefresher : IAuthTokenRefresher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthTokenRefresher(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<Result<TokenResponse>> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result.Failure<TokenResponse>(Error.Validation("Auth.Refresh.MissingToken", "Refresh token is required."));

        var httpClient = _httpClientFactory.CreateClient();
        var tokenEndpoint = $"{_configuration["Authentication:Authority"]}/protocol/openid-connect/token";
        var clientId = _configuration["Authentication:ClientId"];
        var clientSecret = _configuration["Authentication:ClientSecret"];

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        };

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(form), ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result.Failure<TokenResponse>(Error.Problem("Auth.HttpError", $"HTTP request failed: {ex.Message}"));
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return Result.Failure<TokenResponse>(Error.Problem("Auth.RefreshFailed", $"Token refresh failed: {response.StatusCode} - {body}"));
        }

        TokenResponse? tokens;
        try
        {
            tokens = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result.Failure<TokenResponse>(Error.Problem("Auth.ParseFailed", $"Failed to parse token response: {ex.Message}"));
        }

        if (tokens is null || string.IsNullOrEmpty(tokens.AccessToken))
            return Result.Failure<TokenResponse>(Error.Problem("Auth.EmptyToken", "Token response did not contain an access token."));

        return Result.Success(tokens);
    }
}
