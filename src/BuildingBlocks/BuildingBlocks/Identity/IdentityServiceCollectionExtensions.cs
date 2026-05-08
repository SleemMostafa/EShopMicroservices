using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Identity;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddCurrentUserProvider(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

        return services;
    }
}
