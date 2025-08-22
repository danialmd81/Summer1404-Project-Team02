using System.Security.Claims;
using System.Text.Json;

namespace ETL.API.Middlewares;
public class KeycloakClaimsMiddleware
{
    private readonly RequestDelegate _next;

    public KeycloakClaimsMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var identity = (ClaimsIdentity)context.User.Identity;

            // Extract realm roles
            var realmAccess = context.User.FindFirst("realm_access")?.Value;
            if (!string.IsNullOrEmpty(realmAccess))
            {
                using var doc = JsonDocument.Parse(realmAccess);
                if (doc.RootElement.TryGetProperty("roles", out var roles))
                {
                    foreach (var r in roles.EnumerateArray())
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, r.GetString()!));
                    }
                }
            }
        }

        await _next(context);
    }
}

public static class KeycloakClaimsMiddlewareExtensions
{
    public static IApplicationBuilder UseKeycloakClaims(this IApplicationBuilder app)
        => app.UseMiddleware<KeycloakClaimsMiddleware>();
}