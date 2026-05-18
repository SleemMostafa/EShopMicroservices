using Microsoft.AspNetCore.Builder;
using BuildingBlocks.Behaviors;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Resilience;

public static class EshopResilienceExtensions
{
    public static IServiceCollection AddEshopCorrelationId(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CorrelationIdOptions>(
            configuration.GetSection(CorrelationIdOptions.SectionName));
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdDelegatingHandler>();
        services.ConfigureHttpClientDefaults(http =>
            http.AddHttpMessageHandler<CorrelationIdDelegatingHandler>());

        return services;
    }

    public static IServiceCollection AddEshopIdempotency(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<IdempotencyOptions>(
            configuration.GetSection(IdempotencyOptions.SectionName));
        services.AddMemoryCache();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));

        return services;
    }

    public static IApplicationBuilder UseEshopCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
