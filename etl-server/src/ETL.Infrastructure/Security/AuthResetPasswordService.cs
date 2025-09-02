using System.Text;
using System.Text.Json;
using ETL.Application.Abstractions.Security;
using ETL.Application.Common;
using Microsoft.Extensions.Configuration;

namespace ETL.Infrastructure.Security;

public class AuthRestPasswordService : IAuthRestPasswordService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IAdminTokenService _adminTokenService;

    public AuthRestPasswordService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IAdminTokenService adminTokenService)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _adminTokenService = adminTokenService ?? throw new ArgumentNullException(nameof(adminTokenService)); ;
    }

    public async Task<Result> ResetPasswordAsync(string userId, string newPassword, CancellationToken ct = default)
    {
        var adminAccessToken = await _adminTokenService.GetAdminAccessTokenAsync(ct);
        if (string.IsNullOrEmpty(adminAccessToken))
            throw new InvalidOperationException("Could not obtain admin credentials.");

        var httpClient = _httpClientFactory.CreateClient();
        var keycloakBaseUrl = _configuration["Authentication:KeycloakBaseUrl"];
        var realm = _configuration["Authentication:Realm"];

        var resetPasswordUrl = $"{keycloakBaseUrl}/admin/realms/{realm}/users/{userId}/reset-password";

        var resetPasswordPayload = new { type = "password", temporary = false, value = newPassword };

        var jsonContent = new StringContent(JsonSerializer.Serialize(resetPasswordPayload), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Put, resetPasswordUrl);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminAccessToken);
        request.Content = jsonContent;

        var response = await httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            return Result.Failure(Error.Failure("OAuth.ResetPasswordFailed", $"Reset password failed: {err}"));
        }

        return Result.Success();
    }
}
