using System.Data;
using ETL.Application.Abstractions.Data;
using ETL.Application.Abstractions.Repositories;
using ETL.Application.Abstractions.Security;
using ETL.Application.Abstractions.UserServices;
using ETL.Infrastructure.Data;
using ETL.Infrastructure.Data.Abstractions;
using ETL.Infrastructure.OAuthClients;
using ETL.Infrastructure.OAuthClients.Abstractions;
using ETL.Infrastructure.Repositories;
using ETL.Infrastructure.Security;
using ETL.Infrastructure.UserServices;
using ETL.Infrastructure.UserServices.Abstractions;
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

        services.AddSingleton<IOAuthGetJson, OAuthGetJsonClient>();
        services.AddSingleton<IOAuthGetJsonArray, OAuthGetJsonArrayClient>();
        services.AddSingleton<IOAuthPostJson, OAuthPostJsonClient>();
        services.AddSingleton<IOAuthPutJson, OAuthPutJsonClient>();
        services.AddSingleton<IOAuthDeleteJson, OAuthDeleteJsonClient>();
        services.AddSingleton<IOAuthPostJsonWithResponse, OAuthPostJsonWithResponseClient>();

        services.AddSingleton<IUserFetcher, UserFetcher>();
        services.AddSingleton<IUserJsonMapper, UserJsonMapper>();
        services.AddSingleton<IUsersRoleFetcher, UsersRoleFetcher>();
        services.AddSingleton<IUserRoleAssigner, UserRoleAssigner>();

        services.AddSingleton<IOAuthUserReader, OAuthUserReader>();
        services.AddSingleton<IOAuthAllUserReader, OAuthAllUserReader>();
        services.AddSingleton<IOAuthUserRoleGetter, OAuthUserRoleGetter>();
        services.AddSingleton<IOAuthUserCreator, OAuthUserCreator>();
        services.AddSingleton<IOAuthUserUpdater, OAuthUserUpdater>();
        services.AddSingleton<IOAuthUserDeleter, OAuthUserDeleter>();
        services.AddSingleton<IOAuthRoleRemover, OAuthRoleRemover>();
        services.AddSingleton<IOAuthRoleAssigner, OAuthRoleAssigner>();
        services.AddSingleton<IOAuthUserRoleChanger, OAuthUserRoleChanger>();
        services.AddSingleton<IAdminTokenService, AdminTokenService>();
        services.AddSingleton<IAuthCodeForTokenExchanger, AuthCodeForTokenExchanger>();
        services.AddSingleton<IAuthTokenRefresher, AuthTokenRefresher>();
        services.AddSingleton<IAuthCredentialValidator, AuthCredentialValidator>();
        services.AddSingleton<IAuthRestPasswordService, AuthRestPasswordService>();
        services.AddSingleton<IAuthLogoutService, AuthLogoutService>();

        services.AddScoped<Compiler, PostgresCompiler>();
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
