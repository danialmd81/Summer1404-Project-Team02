using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using ETL.Application.Abstractions.Security;
using ETL.Application.Common;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.OAuth;

[ExcludeFromCodeCoverage]
public abstract class OAuthHttpClientBase
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _configuration;
    private readonly IAdminTokenService _adminTokenService;

    protected OAuthHttpClientBase(IHttpClientFactory httpFactory, IConfiguration configuration, IAdminTokenService adminTokenService)
    {
        _httpFactory = httpFactory ?? throw new ArgumentNullException(nameof(httpFactory));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _adminTokenService = adminTokenService ?? throw new ArgumentNullException(nameof(adminTokenService));
    }

    protected string BuildUrl(string relativePath)
    {
        var baseUrl = _configuration["Authentication:KeycloakBaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("Authentication:KeycloakBaseUrl is not configured.");

        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("relativePath is required", nameof(relativePath));

        if (relativePath.StartsWith("/"))
            return $"{baseUrl}{relativePath}";

        return $"{baseUrl}/{relativePath}";
    }

    protected HttpClient CreateClientWithToken(string token)
    {
        var client = _httpFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected async Task<Result<string>> GetAdminTokenAsync(CancellationToken ct = default)
    {
        var token = await _adminTokenService.GetAdminAccessTokenAsync(ct);
        if (string.IsNullOrEmpty(token))
            return Result.Failure<string>(Error.Problem("OAuth.AdminTokenMissing", "Could not obtain admin token"));

        return Result.Success(token);
    }

    protected async Task<Result<JsonElement>> ParseResponseJsonAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Result.Failure<JsonElement>(Error.NotFound("OAuth.NotFound", $"Resource not found: {body}"));

            return Result.Failure<JsonElement>(Error.Problem("OAuth.RequestFailed", $"Request failed: {resp.StatusCode} - {body}"));
        }

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        return Result.Success(doc.RootElement.Clone());
    }
}
