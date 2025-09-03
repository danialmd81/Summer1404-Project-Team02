using System.Net.Http.Json;
using ETL.Application.Abstractions.Security;
using ETL.Application.Common;
using ETL.Application.Common.Options;
using ETL.Infrastructure.OAuth.Abstractions;
using Microsoft.Extensions.Options;

namespace ETL.Infrastructure.OAuth
{
    public class OAuthPutJsonClient : OAuthHttpClientBase, IOAuthPutJson
    {
        public OAuthPutJsonClient(IHttpClientFactory httpFactory, IAdminTokenService adminTokenService, IOptions<AuthOptions> options)
            : base(httpFactory, adminTokenService, options)
        {
        }

        public async Task<Result> PutJsonAsync(string relativePath, object content, CancellationToken ct = default)
        {
            var tokenRes = await GetAdminTokenAsync(ct);
            if (tokenRes.IsFailure) return Result.Failure(tokenRes.Error);

            var url = BuildUrl(relativePath);
            var client = CreateClientWithToken(tokenRes.Value);

            using var req = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = JsonContent.Create(content)
            };

            var resp = await client.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return Result.Failure(Error.NotFound("OAuth.NotFound", $"Resource not found: {url}"));

                return Result.Failure(Error.Problem("OAuth.RequestFailed", $"PUT {url} failed: {resp.StatusCode} - {body}"));
            }

            return Result.Success();
        }
    }
}
