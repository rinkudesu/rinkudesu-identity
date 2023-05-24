using System.Diagnostics.CodeAnalysis;
using Rinkudesu.Identity.Service.Settings;
using StackExchange.Redis;

namespace Rinkudesu.Identity.Service.Services;

[ExcludeFromCodeCoverage]
public class RedisConnectionProvider
{
    private readonly ConnectionMultiplexer _redisMultiplexer;

    public RedisConnectionProvider(ConnectionMultiplexer redisMultiplexer)
    {
        _redisMultiplexer = redisMultiplexer;
    }

    public virtual IServer GetServer() => _redisMultiplexer.GetServer(RedisSettings.Current.Address);
    public virtual IDatabase GetDatabase() => _redisMultiplexer.GetDatabase();
}
