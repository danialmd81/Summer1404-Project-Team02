using System.Security.Claims;

namespace ETL.API.Middleware;
// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
public class RoleClaimsMiddleware
{
    private readonly RequestDelegate _next;

    public RoleClaimsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var roleClaims = user.FindAll("roles").Select(c => c.Value).ToList();
            var claims = new List<Claim>(user.Claims)
            {
                new Claim(ClaimTypes.Role, string.Join(",", roleClaims))
            };

            var identity = new ClaimsIdentity(claims, "jwt");
            context.User = new ClaimsPrincipal(identity);
        }

        await _next(context);
    }
}

