using System.Net.Http.Json;
using ETL.Application.Abstractions.Security;
using ETL.Application.Common;
using ETL.Application.Common.Options;
using ETL.Infrastructure.OAuth.Abstractions;
using Microsoft.Extensions.Options;

namespace ETL.Infrastructure.OAuth;

public class OAuthDeleteJsonClient : OAuthHttpClientBase, IOAuthDeleteJson
{
    public OAuthDeleteJsonClient(IHttpClientFactory httpFactory, IAdminTokenService adminTokenService, IOptions<AuthOptions> options)
        : base(httpFactory, adminTokenService, options)
    {
    }

    public async Task<Result> DeleteJsonAsync(string relativePath, object? content = null, CancellationToken ct = default)
    {
        var tokenRes = await GetAdminTokenAsync(ct);
        if (tokenRes.IsFailure) return Result.Failure(tokenRes.Error);

        var url = BuildUrl(relativePath);
        var client = CreateClientWithToken(tokenRes.Value);

        using var req = new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = content is null ? null : JsonContent.Create(content)
        };

        var resp = await client.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            return Result.Failure(Error.Problem("OAuth.RequestFailed", $"DELETE {url} failed: {resp.StatusCode} - {body}"));
        }

        return Result.Success();
    }
}
