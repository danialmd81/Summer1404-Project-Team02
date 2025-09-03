using ETL.Application.Common.Options;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ETL.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration config)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.Configure<AuthOptions>(config.GetSection(AuthOptions.SectionName));
        services.Configure<OAuthAdminOptions>(config.GetSection(OAuthAdminOptions.SectionName));
        return services;
    }
}
