using System.Diagnostics.CodeAnalysis;
using Rinkudesu.Identity.Service.Settings;
using StackExchange.Redis;

namespace Rinkudesu.Identity.Service.Services;

/// <summary>
/// Wrapper for <see cref="ConnectionMultiplexer"/> simplifying Redis connection establishing.
/// </summary>
[ExcludeFromCodeCoverage]
public class RedisConnectionProvider
{
    private readonly ConnectionMultiplexer _redisMultiplexer;

    /// <summary>
    /// Initialises provider using the given multiplexer.
    /// </summary>
    public RedisConnectionProvider(ConnectionMultiplexer redisMultiplexer)
    {
        _redisMultiplexer = redisMultiplexer;
    }

    /// <summary>
    /// Returns Redis server.
    /// </summary>
    public virtual IServer GetServer() => _redisMultiplexer.GetServer(RedisSettings.Current.Address);
    /// <summary>
    /// Returns Redis database.
    /// </summary>
    public virtual IDatabase GetDatabase() => _redisMultiplexer.GetDatabase();
}
