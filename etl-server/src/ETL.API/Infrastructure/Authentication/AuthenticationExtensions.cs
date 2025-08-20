using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ETL.API.Infrastructure.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddKeycloakStatelessCookieAuth(this IServiceCollection services, IConfiguration config)
    {
        var section = config.GetSection("Authentication");
        var authority = section["Authority"]!;
        var clientId = section["ClientId"]!;
        const string cookieName = "__Host-auth-token";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = clientId;
                options.RequireHttpsMetadata = false; // Dev only
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role, // Use the ClaimTypes constant for the role
                    ValidateAudience = true,
                };

                options.Events = new JwtBearerEvents
                {
                    // First, read the token from the cookie
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue(cookieName, out var token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    },
                    // Then, after validating the token, parse the roles
                    OnTokenValidated = context =>
                    {
                        if (context.Principal?.Identity is ClaimsIdentity identity &&
                            context.Principal.HasClaim(c => c.Type == "realm_access"))
                        {
                            var realmAccessClaim = context.Principal.FindFirst("realm_access")!.Value;
                            using var realmAccessDoc = JsonDocument.Parse(realmAccessClaim);
                            var realmRoles = realmAccessDoc.RootElement.GetProperty("roles").EnumerateArray();

                            foreach (var role in realmRoles)
                            {
                                // Add each role as a new claim with the standard Role type
                                identity.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        
        // ... your antiforgery setup ...
        return services;
    }
}