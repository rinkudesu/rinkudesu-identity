using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyModel;
using Rinkudesu.Identity.Service.Utilities;

namespace Rinkudesu.Gateways.Webui.Models;

/// <summary>
/// Defines settings used for connection to Redis cache.
/// </summary>
[ExcludeFromCodeCoverage]
public class RedisSettings
{
    private static RedisSettings? current;

    /// <summary>
    /// Represents the currently valid Redis connection settings. Can only be set once.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing values that have not yet been initialised or trying to initialise values that have already been initialised.</exception>
    public static RedisSettings Current
    {
        get => current ?? throw new InvalidOperationException("Redis settings have not been initialised");
        set
        {
            if (current is not null)
                throw new InvalidOperationException("Redis settings have already been initialised");

            current = value;
        }
    }

    /// <summary>
    /// Full Redis connection address in format host:port.
    /// </summary>
    public string Address { get; }
    /// <summary>
    /// Only the host part of <see cref="Address"/>
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// Creates new Redis connection settings using predefined env variables.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when any setting value is missing from env</exception>
    public RedisSettings()
    {
        Address = EnvironmentalVariablesReader.GetRequiredVariable(EnvironmentalVariablesReader.RedisAddressVariableName);
        Host = Address.Split(':')[0];
    }

    /// <summary>
    /// Returns current settings in <see cref="RedisCacheOptions"/> format.
    /// </summary>
    [SuppressMessage("Design", "CA1024:Use properties where appropriate")] // this is most certainly not appropriate
    public RedisCacheOptions GetRedisOptions() => new RedisCacheOptions { Configuration = Address };
}
