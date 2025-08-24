using ETL.Application.Common.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ETL.Infrastructure.Security;

public static class AuthorizationExtension
{
    public static IServiceCollection AddPolicyAuthorization(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policy.SystemAdminOnly, policy =>
                policy.RequireRole(Role.SystemAdmin));

            options.AddPolicy(Policy.DataAdminOnly, policy =>
                policy.RequireRole(Role.DataAdmin));

            options.AddPolicy(Policy.AnalystOnly, policy =>
                policy.RequireRole(Policy.AnalystOnly));

            options.AddPolicy(Policy.CanManageUsers, policy =>
                policy.RequireRole(Role.SystemAdmin));

            options.AddPolicy(Policy.AuthenticatedUser, policy =>
                policy.RequireAuthenticatedUser());
        });

        return services;
    }
}