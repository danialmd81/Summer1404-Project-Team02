using System.Data;
using ETL.Application.Abstractions.Data;
using ETL.Application.Abstractions.Repositories;
using ETL.Application.Abstractions.Security;
using ETL.Application.Abstractions.UserServices;
using ETL.Infrastructure.Data;
using ETL.Infrastructure.OAuth;
using ETL.Infrastructure.OAuth.Abstractions;
using ETL.Infrastructure.Repository;
using ETL.Infrastructure.Security;
using ETL.Infrastructure.UserServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace ETL.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddOAuth(config);
        services.AddPolicyAuthorization(config);

        services.AddHttpClient(string.Empty)
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    // ⚠️ WARNING: DANGEROUS - FOR DEVELOPMENT ONLY
                    // This handler bypasses SSL certificate validation.
                    return new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                });

        services.AddScoped<IOAuthGetJson, OAuthGetJsonClient>();
        services.AddScoped<IOAuthGetJsonArray, OAuthGetJsonArrayClient>();
        services.AddScoped<IOAuthPostJson, OAuthPostJsonClient>();
        services.AddScoped<IOAuthPutJson, OAuthPutJsonClient>();
        services.AddScoped<IOAuthDeleteJson, OAuthDeleteJsonClient>();
        services.AddScoped<IOAuthPostJsonWithResponse, OAuthPostJsonWithResponseClient>();

        services.AddScoped<IOAuthUserReader, OAuthUserReader>();
        services.AddScoped<IOAuthAllUserReader, OAuthAllUserReader>();
        services.AddScoped<IOAuthUserRoleGetter, OAuthUserRoleGetter>();
        services.AddScoped<IOAuthUserCreator, OAuthUserCreator>();
        services.AddScoped<IOAuthUserUpdater, OAuthUserUpdater>();
        services.AddScoped<IOAuthUserDeleter, OAuthUserDeleter>();
        services.AddScoped<IRoleRemover, RoleRemover>();
        services.AddScoped<IOAuthRoleAssigner, OAuthRoleAssigner>();
        services.AddScoped<IOAuthUserRoleChanger, OAuthUserRoleChanger>();
        services.AddScoped<IAdminTokenService, AdminTokenService>();
        services.AddScoped<IAuthCodeForTokenExchanger, AuthCodeForTokenExchanger>();
        services.AddScoped<ITokenRefresher, AuthTokenRefresher>();
        services.AddScoped<IAuthCredentialValidator, AuthCredentialValidator>();
        services.AddScoped<IAuthRestPasswordService, AuthRestPasswordService>();
        services.AddScoped<IAuthLogoutService, AuthLogoutService>();
        services.AddScoped<IDbConnection>(sp =>
        {
            return new NpgsqlConnection(config.GetConnectionString("DefaultConnection"));
        });
        services.AddScoped<IDynamicTableRepository, DynamicTableRepository>();
        services.AddScoped<IDataSetRepository, DataSetRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
