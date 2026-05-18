using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Authentication;

public static class InternalServiceAuthExtensions
{
    public static IServiceCollection AddInternalServiceTokenProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<InternalServiceAuthOptions>(
            configuration.GetSection(InternalServiceAuthOptions.SectionName));
        services.AddSingleton<IInternalServiceTokenProvider, InternalServiceTokenProvider>();

        return services;
    }

    public static IServiceCollection AddInternalServiceAuthorization(
        this IServiceCollection services,
        string policyName,
        string allowedService,
        string requiredScope)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(policyName, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(InternalServiceClaims.TokenUse, InternalServiceClaims.ServiceTokenUse)
                .RequireClaim(InternalServiceClaims.Service, allowedService)
                .RequireClaim(InternalServiceClaims.Scope, requiredScope));
        });

        return services;
    }
}
