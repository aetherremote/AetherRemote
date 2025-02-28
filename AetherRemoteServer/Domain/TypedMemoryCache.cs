using Microsoft.Extensions.Caching.Memory;

namespace AetherRemoteServer.Domain;

/// <summary>
/// Wrapper for <see cref="MemoryCache"/> to allow for strict typing
/// </summary>
public class TypedMemoryCache<T>
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly TimeSpan _expiration = TimeSpan.FromMinutes(20);

    /// <summary>
    /// Sets a cache entry with the given key and value
    /// </summary>
    public void Set(string key, T value)
    {
        _cache.Set(key, value, _expiration);
    }

    /// <summary>
    /// Gets the value associated with the given key
    /// </summary>
    public T? Get(string key)
    {
        return _cache.TryGetValue(key, out T? value) ? value : default;
    }

    /// <summary>
    /// Removes a cache entry
    /// </summary>
    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}