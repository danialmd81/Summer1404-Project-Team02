using System.Text.Json;
using ETL.Application.Abstractions.Security;
using ETL.Application.Common;
using ETL.Infrastructure.OAuth.Abstractions;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.OAuth
{
    public class OAuthGetJsonClient : OAuthHttpClientBase, IOAuthGetJson
    {
        public OAuthGetJsonClient(IHttpClientFactory httpFactory, IConfiguration configuration, IAdminTokenService adminTokenService)
            : base(httpFactory, configuration, adminTokenService)
        {
        }

        public async Task<Result<JsonElement>> GetJsonAsync(string relativePath, CancellationToken ct = default)
        {
            var tokenRes = await GetAdminTokenAsync(ct);
            if (tokenRes.IsFailure) return Result.Failure<JsonElement>(tokenRes.Error);

            var url = BuildUrl(relativePath);
            var client = CreateClientWithToken(tokenRes.Value);

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            var resp = await client.SendAsync(req, ct);

            return await ParseResponseJsonAsync(resp, ct);
        }
    }
}
