using System.Text.Json;
using ETL.Contracts.Security;

namespace ETL.API.Middlewares;

public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _clientFactory;

    public TokenRefreshMiddleware(RequestDelegate next, IConfiguration config, IHttpClientFactory clientFactory)
    {
        _next = next;
        _config = config;
        _clientFactory = clientFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var refreshToken = context.Request.Cookies["refresh_token"];
        var accessToken = context.Request.Cookies["access_token"];

        if (!string.IsNullOrEmpty(refreshToken) && (string.IsNullOrEmpty(accessToken)))
        {
            var tokenEndpoint = $"{_config["Authentication:Authority"]}/protocol/openid-connect/token";
            var clientId = _config["Authentication:ClientId"];
            var clientSecret = _config["Authentication:ClientSecret"];

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret
            };

            var http = _clientFactory.CreateClient();
            var response = await http.PostAsync(tokenEndpoint, new FormUrlEncodedContent(form));

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var tokens = JsonSerializer.Deserialize<TokenResponse>(json);

                if (tokens?.AccessToken != null)
                {
                    var accessOpts = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddSeconds(tokens.AccessExpiresIn)
                    };
                    context.Response.Cookies.Append("access_token", tokens.AccessToken, accessOpts);

                    if (tokens.RefreshToken != null)
                    {
                        var refreshOpts = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddSeconds(tokens.RefreshExpiresIn)
                        };
                        context.Response.Cookies.Append("refresh_token", tokens.RefreshToken, refreshOpts);
                    }

                    await _next(context);
                    return;
                }
            }


            context.Response.Cookies.Delete("access_token");
            context.Response.Cookies.Delete("refresh_token");

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Session expired. Please log in again." });
            return;
        }

        await _next(context);
    }
}

public static class TokenRefreshMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenRefresh(this IApplicationBuilder app)
     => app.UseMiddleware<TokenRefreshMiddleware>();
}

