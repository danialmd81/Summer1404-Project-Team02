using Microsoft.Extensions.DependencyInjection;

namespace ETL.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Register your infrastructure services here
        // Example: services.AddSingleton<IMyService, MyService>();

        return services;
    }
}
