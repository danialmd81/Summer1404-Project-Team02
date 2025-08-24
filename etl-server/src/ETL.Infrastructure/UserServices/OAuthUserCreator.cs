using System.Net.Http.Json;
using ETL.Application.Abstractions;
using ETL.Application.Abstractions.Security;
using ETL.Application.Common;
using ETL.Application.User.Create;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.UserServices
{
    public class OAuthUserCreator : IOAuthUserCreator
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _configuration;
        private readonly IAdminTokenService _adminTokenService;

        public OAuthUserCreator(IHttpClientFactory httpFactory, IConfiguration configuration, IAdminTokenService adminTokenService)
        {
            _httpFactory = httpFactory;
            _configuration = configuration;
            _adminTokenService = adminTokenService;
        }

        public async Task<Result<string>> CreateUserAsync(CreateUserCommand command, CancellationToken ct = default)
        {
            var adminToken = await _adminTokenService.GetAdminAccessTokenAsync(ct);
            if (string.IsNullOrEmpty(adminToken))
                return Result.Failure<string>(Error.Problem("Keycloak.AdminTokenMissing", "Could not obtain admin token"));

            var httpClient = _httpFactory.CreateClient();
            var keycloakBaseUrl = _configuration["Authentication:KeycloakBaseUrl"]?.TrimEnd('/');
            var realm = _configuration["Authentication:Realm"];

            var createUserUrl = $"{keycloakBaseUrl}/admin/realms/{realm}/users";

            var newUserPayload = new
            {
                username = command.Username,
                email = command.Email,
                firstName = command.FirstName,
                lastName = command.LastName,
                enabled = true,
                credentials = new[]
                {
                    new { type = "password", value = command.Password, temporary = false }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, createUserUrl)
            {
                Content = JsonContent.Create(newUserPayload)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var response = await httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                return Result.Failure<string>(Error.Problem("Keycloak.CreateUserFailed", $"Create user failed: {response.StatusCode} - {body}"));
            }

            var location = response.Headers.Location;
            if (location == null)
            {
                return Result.Failure<string>(Error.Problem("Keycloak.NoLocationHeader", "Keycloak did not return Location header for created user."));
            }

            string newUserId;
            try
            {
                var segments = location.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                newUserId = segments[^1];
            }
            catch
            {
                return Result.Failure<string>(Error.Problem("Keycloak.ParseUserIdFailed", "Failed to parse created user id from Location header."));
            }

            return Result.Success(newUserId);
        }
    }
}
