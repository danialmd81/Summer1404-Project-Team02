using System.Text.Json;
using ETL.Application.Abstractions.Security;
using ETL.Application.Common;
using ETL.Infrastructure.OAuth.Abstractions;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.OAuth;

public class OAuthGetJsonArrayClient : OAuthHttpClientBase, IOAuthGetJsonArray
{
    public OAuthGetJsonArrayClient(IHttpClientFactory httpFactory, IConfiguration configuration, IAdminTokenService adminTokenService)
        : base(httpFactory, configuration, adminTokenService)
    {
    }

    public async Task<Result<List<JsonElement>>> GetJsonArrayAsync(string relativePath, CancellationToken ct = default)
    {
        var tokenRes = await GetAdminTokenAsync(ct);
        if (tokenRes.IsFailure) return Result.Failure<List<JsonElement>>(tokenRes.Error);

        var url = BuildUrl(relativePath);
        var client = CreateClientWithToken(tokenRes.Value);

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        var resp = await client.SendAsync(req, ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Result.Failure<List<JsonElement>>(Error.NotFound("OAuth.NotFound", $"Resource not found: {url}"));

            return Result.Failure<List<JsonElement>>(Error.Problem("OAuth.RequestFailed", $"GET {url} failed: {resp.StatusCode} - {body}"));
        }

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var list = new List<JsonElement>();
        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var el in doc.RootElement.EnumerateArray())
                list.Add(el.Clone());
        }

        return Result.Success(list);
    }
}
