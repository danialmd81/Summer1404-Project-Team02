using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace ETL.API.Infrastructure.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var section = config.GetSection("Authentication");
        var authority = section["Authority"]!;
        var clientId = section["ClientId"]!;
        var clientSecret = section["ClientSecret"]!;

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = authority; // http://localhost:8888/realms/etl-realm
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.ResponseType = OpenIdConnectResponseType.Code;

                options.SaveTokens = true;
                options.RequireHttpsMetadata = false; // only for dev
                
                options.GetClaimsFromUserInfoEndpoint = true;

                // This must match what we put in Keycloak
                options.CallbackPath = "/signin-oidc";  

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "preferred_username",
                    RoleClaimType = "role"
                };
            });

        return services;
    }
}