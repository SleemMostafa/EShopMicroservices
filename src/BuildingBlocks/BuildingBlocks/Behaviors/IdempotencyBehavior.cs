using System.Text.Json;
using BuildingBlocks.CQRS;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Behaviors;

public sealed class IdempotencyBehavior<TRequest, TResponse>(
    IServiceProvider serviceProvider,
    IMemoryCache memoryCache,
    IOptions<IdempotencyOptions> options,
    ILogger<IdempotencyBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled ||
            request is not IIdempotentCommand<TResponse> idempotentCommand ||
            string.IsNullOrWhiteSpace(idempotentCommand.IdempotencyKey))
        {
            return await next();
        }

        var cacheKey = $"idempotency:{typeof(TRequest).FullName}:{idempotentCommand.IdempotencyKey}";
        var distributedCache = serviceProvider.GetService<IDistributedCache>();
        var cachedResponse = await TryGetCachedResponseAsync(distributedCache, cacheKey, cancellationToken);

        if (cachedResponse is not null)
        {
            logger.LogInformation(
                "Returning cached idempotent response for {RequestName} with key {IdempotencyKey}",
                typeof(TRequest).Name,
                idempotentCommand.IdempotencyKey);

            return cachedResponse;
        }

        var response = await next();
        await StoreResponseAsync(distributedCache, cacheKey, response, cancellationToken);

        return response;
    }

    private async Task<TResponse?> TryGetCachedResponseAsync(
        IDistributedCache? distributedCache,
        string cacheKey,
        CancellationToken cancellationToken)
    {
        if (distributedCache is not null)
        {
            var cached = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
            return string.IsNullOrWhiteSpace(cached)
                ? default
                : JsonSerializer.Deserialize<TResponse>(cached);
        }

        return memoryCache.TryGetValue(cacheKey, out TResponse? response)
            ? response
            : default;
    }

    private async Task StoreResponseAsync(
        IDistributedCache? distributedCache,
        string cacheKey,
        TResponse response,
        CancellationToken cancellationToken)
    {
        var expiration = TimeSpan.FromMinutes(options.Value.ExpirationMinutes);

        if (distributedCache is not null)
        {
            var serialized = JsonSerializer.Serialize(response);
            await distributedCache.SetStringAsync(
                cacheKey,
                serialized,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration },
                cancellationToken);
            return;
        }

        memoryCache.Set(cacheKey, response, expiration);
    }
}
