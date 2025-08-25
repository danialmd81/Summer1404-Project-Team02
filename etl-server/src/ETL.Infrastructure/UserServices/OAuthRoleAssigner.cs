using System.Net.Http.Json;
using System.Text.Json;
using ETL.Application.Abstractions;
using ETL.Application.Abstractions.Security;
using ETL.Application.Common;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.UserServices
{
    public class OAuthRoleAssigner : IOAuthRoleAssigner
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _configuration;
        private readonly IAdminTokenService _adminTokenService;

        public OAuthRoleAssigner(IHttpClientFactory httpFactory, IConfiguration configuration, IAdminTokenService adminTokenService)
        {
            _httpFactory = httpFactory;
            _configuration = configuration;
            _adminTokenService = adminTokenService;
        }

        public async Task<Result> AssignRolesAsync(string userId, IEnumerable<string> roleNames, CancellationToken ct = default)
        {
            var adminToken = await _adminTokenService.GetAdminAccessTokenAsync(ct);
            if (string.IsNullOrEmpty(adminToken))
                return Result.Failure(Error.Problem("OAuth.AdminTokenMissing", "Could not obtain admin token"));

            var httpClient = _httpFactory.CreateClient();
            var keycloakBaseUrl = _configuration["Authentication:KeycloakBaseUrl"]?.TrimEnd('/');
            var realm = _configuration["Authentication:Realm"];

            var roleRepresentations = new List<JsonElement>();

            foreach (var roleName in roleNames)
            {
                var getRoleUrl = $"{keycloakBaseUrl}/admin/realms/{realm}/roles/{Uri.EscapeDataString(roleName)}";
                using var getRoleReq = new HttpRequestMessage(HttpMethod.Get, getRoleUrl);
                getRoleReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

                var roleResp = await httpClient.SendAsync(getRoleReq, ct);
                if (!roleResp.IsSuccessStatusCode)
                {
                    var body = await roleResp.Content.ReadAsStringAsync(ct);
                    return Result.Failure(Error.NotFound("OAuth.RoleNotFound", $"Role '{roleName}' not found: {roleResp.StatusCode} {body}"));
                }

                using var stream = await roleResp.Content.ReadAsStreamAsync(ct);
                var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                roleRepresentations.Add(doc.RootElement.Clone());
            }

            if (roleRepresentations.Count == 0)
                return Result.Success();

            var assignRoleUrl = $"{keycloakBaseUrl}/admin/realms/{realm}/users/{userId}/role-mappings/realm";
            using var assignReq = new HttpRequestMessage(HttpMethod.Post, assignRoleUrl)
            {
                Content = JsonContent.Create(roleRepresentations)
            };
            assignReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var assignResp = await httpClient.SendAsync(assignReq, ct);
            if (!assignResp.IsSuccessStatusCode)
            {
                var body = await assignResp.Content.ReadAsStringAsync(ct);
                return Result.Failure(Error.Problem("OAuth.AssignRolesFailed", $"Assign roles failed: {assignResp.StatusCode} - {body}"));
            }

            return Result.Success();
        }
    }
}
