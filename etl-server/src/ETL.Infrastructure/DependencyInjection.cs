using System.Data;
using ETL.Application.Abstractions.Data;
using ETL.Application.Abstractions.Repositories;
using ETL.Application.Abstractions.Security;
using ETL.Application.Abstractions.UserServices;
using ETL.Infrastructure.Data;
using ETL.Infrastructure.Data.Abstractions;
using ETL.Infrastructure.OAuth;
using ETL.Infrastructure.OAuth.Abstractions;
using ETL.Infrastructure.Repositories;
using ETL.Infrastructure.Security;
using ETL.Infrastructure.UserServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SqlKata.Compilers;

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
        services.AddScoped<IOAuthRoleRemover, OAuthRoleRemover>();
        services.AddScoped<IOAuthRoleAssigner, OAuthRoleAssigner>();
        services.AddScoped<IOAuthUserRoleChanger, OAuthUserRoleChanger>();
        services.AddScoped<IAdminTokenService, AdminTokenService>();
        services.AddScoped<IAuthCodeForTokenExchanger, AuthCodeForTokenExchanger>();
        services.AddScoped<ITokenRefresher, AuthTokenRefresher>();
        services.AddScoped<IAuthCredentialValidator, AuthCredentialValidator>();
        services.AddScoped<IAuthRestPasswordService, AuthRestPasswordService>();
        services.AddScoped<IAuthLogoutService, AuthLogoutService>();

        services.AddSingleton<Compiler, PostgresCompiler>();
        services.AddScoped<IDbExecutor, DapperDbExecutor>();
        services.AddScoped<IPostgresCopyAdapter, PostgresCopyAdapter>();
        services.AddScoped<IQueryCompiler, SqlKataCompilerAdapter>();
        services.AddScoped<IDbConnection>(sp =>
        {
            return new NpgsqlConnection(config.GetConnectionString("DefaultConnection"));
        });
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IStagingTableRepository, StagingTableRepository>();
        services.AddScoped<IDataSetRepository, DataSetRepository>();


        return services;
    }
}
