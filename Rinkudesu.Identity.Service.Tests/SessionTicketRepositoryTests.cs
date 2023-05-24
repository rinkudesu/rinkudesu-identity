using Moq;
using Rinkudesu.Identity.Service.Repositories;
using Rinkudesu.Identity.Service.Services;
using StackExchange.Redis;

namespace Rinkudesu.Identity.Service.Tests;

public class SessionTicketRepositoryTests
{
    private readonly Mock<IServer> _mockServer = new Mock<IServer>(MockBehavior.Strict);
    private readonly Mock<IDatabase> _mockDatabase = new Mock<IDatabase>(MockBehavior.Strict);

    private readonly SessionTicketRepository _ticketRepository;

    public SessionTicketRepositoryTests()
    {
        var redisConnectionProvider = new Mock<RedisConnectionProvider>((ConnectionMultiplexer?)null);
        redisConnectionProvider.Setup(r => r.GetServer()).Returns(_mockServer.Object);
        redisConnectionProvider.Setup(r => r.GetDatabase()).Returns(_mockDatabase.Object);
        _ticketRepository = new SessionTicketRepository(redisConnectionProvider.Object);
    }

    [Fact]
    public async Task RemoveUserSessionTickets_AllUserIdSessionsDeleted()
    {
        var userId = Guid.NewGuid();
        const int count = 10;
        _mockServer.Setup(s
                => s.KeysAsync(
                    It.Is<int>(i => i == -1),
                    It.Is<RedisValue>(v
                        => v.Equals(new RedisValue($"{RedisCacheTicketStore.PREFIX}_{userId.ToString()}_*"))
                    ), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CommandFlags>()))
            .Returns(GetRedisKeys(userId, count)).Verifiable();

        await _ticketRepository.RemoveUserSessionTickets(userId);

        _mockServer.VerifyAll();
        _mockDatabase.VerifyAll();
    }

#pragma warning disable CS1998
    private async IAsyncEnumerable<RedisKey> GetRedisKeys(Guid userId, int count)
#pragma warning restore CS1998
    {
        for (var i = 0; i < count; i++)
        {
            var key = $"{RedisCacheTicketStore.PREFIX}_{userId.ToString()}_{i.ToString()}";
            _mockDatabase.Setup(d => d.KeyDeleteAsync(It.Is<RedisKey>(k => k.Equals(new RedisKey(key))), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true).Verifiable();
            yield return new RedisKey(key);
        }
    }
}
