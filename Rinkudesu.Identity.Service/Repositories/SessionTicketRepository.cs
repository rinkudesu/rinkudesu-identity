using Rinkudesu.Identity.Service.Services;
using StackExchange.Redis;

namespace Rinkudesu.Identity.Service.Repositories;

public class SessionTicketRepository
{
    private readonly IServer _redisServer;
    private readonly IDatabase _redisDatabase;

    public SessionTicketRepository(RedisConnectionProvider redisConnectionProvider)
    {
        _redisServer = redisConnectionProvider.GetServer();
        _redisDatabase = redisConnectionProvider.GetDatabase();
    }

    public async Task RemoveUserSessionTickets(Guid userId, CancellationToken cancellationToken = default)
    {
        var keys = _redisServer.KeysAsync(pattern: $"{RedisCacheTicketStore.PREFIX}_{userId.ToString()}_*");
        await foreach (var key in keys.WithCancellation(cancellationToken))
        {
            await _redisDatabase.KeyDeleteAsync(key);
        }
    }
}
