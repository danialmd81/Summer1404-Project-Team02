namespace ETL.API.Infrastructure;

public static class AuthorizationService
{
    public static IServiceCollection AddKeycloakAuthorization(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.SystemAdminOnly, policy =>
                policy.RequireRole(Roles.SystemAdmin));

            options.AddPolicy(Policies.DataAdminOnly, policy =>
                policy.RequireRole(Roles.DataAdmin));

            options.AddPolicy(Policies.AnalystOnly, policy =>
                policy.RequireRole(Policies.AnalystOnly));

            options.AddPolicy(Policies.CanManageUsers, policy =>
                policy.RequireRole(Roles.SystemAdmin));

            options.AddPolicy(Policies.AuthenticatedUser, policy =>
                policy.RequireAuthenticatedUser());
        });

        return services;
    }
}