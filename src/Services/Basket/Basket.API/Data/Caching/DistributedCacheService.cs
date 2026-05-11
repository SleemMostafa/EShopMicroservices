using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.API.Data.Caching;

public sealed class DistributedCacheService(IDistributedCache cache) : ICacheService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cachedValue = await cache.GetStringAsync(key, cancellationToken);

        return string.IsNullOrWhiteSpace(cachedValue)
            ? default
            : JsonSerializer.Deserialize<T>(cachedValue, JsonSerializerOptions);
    }

    public Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        return cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(value, JsonSerializerOptions),
            cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return cache.RemoveAsync(key, cancellationToken);
    }
}
