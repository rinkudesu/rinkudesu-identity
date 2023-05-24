using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Rinkudesu.Identity.Service.Common.Utilities;

namespace Rinkudesu.Identity.Service.Services;

/// <summary>
/// Provides a session ticket storage using Redis cache.
/// </summary>
[ExcludeFromCodeCoverage]
public class RedisCacheTicketStore : ITicketStore, IDisposable
{
    public const string PREFIX = "SessionTicket";

    private readonly RedisCache _cache;

    /// <summary>
    /// Creates a new session ticket storage using Redis.
    /// </summary>
    public RedisCacheTicketStore(RedisCacheOptions options)
    {
        _cache = new RedisCache(options);
    }

    /// <inheritdoc/>
    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var random = RandomNumberGenerator.GetBytes(256);
        var key = $"{PREFIX}_{ticket.Principal.GetUserId()}_{Convert.ToBase64String(random)}";
        await SetTicket(key, ticket);
        return key;
    }

    /// <inheritdoc/>
    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        await SetTicket(key, ticket);
    }

    /// <inheritdoc/>
    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var bytes = await _cache.GetAsync(key);
        if (bytes is null)
            return null;
        return TicketSerializer.Default.Deserialize(bytes);
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    private async Task SetTicket(string id, AuthenticationTicket ticket)
    {
        var cacheOptions = new DistributedCacheEntryOptions();
        var ticketBytes = TicketSerializer.Default.Serialize(ticket);
        if (ticket.Properties.ExpiresUtc is {} expiration)
            cacheOptions.AbsoluteExpiration = expiration;
        await _cache.SetAsync(id, ticketBytes, cacheOptions);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cache.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
