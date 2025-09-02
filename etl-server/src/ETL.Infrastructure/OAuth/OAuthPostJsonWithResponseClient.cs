using System.Net.Http.Json;
using ETL.Application.Abstractions.Security;
using ETL.Application.Common;
using ETL.Infrastructure.OAuth.Abstractions;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.OAuth;

public class OAuthPostJsonWithResponseClient : OAuthHttpClientBase, IOAuthPostJsonWithResponse
{
    public OAuthPostJsonWithResponseClient(IHttpClientFactory httpFactory, IConfiguration configuration, IAdminTokenService adminTokenService)
        : base(httpFactory, configuration, adminTokenService)
    {
    }

    public async Task<Result<HttpResponseMessage>> PostJsonForResponseAsync(string relativePath, object content, CancellationToken ct = default)
    {
        var tokenRes = await GetAdminTokenAsync(ct);
        if (tokenRes.IsFailure) return Result.Failure<HttpResponseMessage>(tokenRes.Error);

        var url = BuildUrl(relativePath);
        var client = CreateClientWithToken(tokenRes.Value);

        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(content)
        };

        HttpResponseMessage resp;
        try
        {
            resp = await client.SendAsync(req, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result.Failure<HttpResponseMessage>(Error.Problem("OAuth.HttpError", $"HTTP request failed: {ex.Message}"));
        }

        return Result.Success(resp);
    }
}
