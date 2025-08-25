using ETL.Application.Abstractions;
using ETL.Application.Abstractions.Security;
using ETL.Application.Common;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.UserServices
{
    public class OAuthUserDeleter : IOAuthUserDeleter
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _configuration;
        private readonly IAdminTokenService _adminTokenService;

        public OAuthUserDeleter(IHttpClientFactory httpFactory, IConfiguration configuration, IAdminTokenService adminTokenService)
        {
            _httpFactory = httpFactory;
            _configuration = configuration;
            _adminTokenService = adminTokenService;
        }

        public async Task<Result> DeleteUserAsync(string userId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result.Failure(Error.Failure("OAuth.Delete.InvalidUserId", "User id is required"));

            var adminToken = await _adminTokenService.GetAdminAccessTokenAsync(ct);
            if (string.IsNullOrEmpty(adminToken))
                return Result.Failure(Error.Problem("OAuth.AdminTokenMissing", "Could not obtain admin token"));

            var httpClient = _httpFactory.CreateClient();
            var keycloakBaseUrl = _configuration["Authentication:KeycloakBaseUrl"]?.TrimEnd('/');
            var realm = _configuration["Authentication:Realm"];

            if (string.IsNullOrWhiteSpace(keycloakBaseUrl) || string.IsNullOrWhiteSpace(realm))
                return Result.Failure(Error.Problem("OAuth.ConfigurationMissing", "OAuth base url or realm is not configured"));

            var deleteUrl = $"{keycloakBaseUrl}/admin/realms/{realm}/users/{Uri.EscapeDataString(userId)}";
            using var req = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            HttpResponseMessage resp;
            try
            {
                resp = await httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                return Result.Failure(Error.Problem("OAuth.HttpError", $"HTTP request to OAuth failed: {ex.Message}"));
            }

            if (resp.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                return Result.Failure(Error.NotFound("OAuth.UserNotFound", $"User '{userId}' not found: {body}"));
            }

            var respBody = await resp.Content.ReadAsStringAsync(ct);
            return Result.Failure(Error.Problem("OAuth.DeleteFailed", $"Failed to delete user '{userId}': {(int)resp.StatusCode} {resp.ReasonPhrase} - {respBody}"));
        }
    }
}
