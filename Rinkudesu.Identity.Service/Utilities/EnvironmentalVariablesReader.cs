using System.Diagnostics.CodeAnalysis;

namespace Rinkudesu.Identity.Service.Utilities;

/// <summary>
/// Helper class used for env variable retrieval.
/// </summary>
[ExcludeFromCodeCoverage]
public static class EnvironmentalVariablesReader
{
    /// <summary>
    /// Name of the variable storing the database connection string.
    /// </summary>
    public const string DbContextVariableName = "RINKU_IDENTITY_CONNECTIONSTRING";
    /// <summary>
    /// Name of the variable storing the redis connection string.
    /// </summary>
    public const string RedisAddressVariableName = "RINKU_IDENTITY_REDIS";
    /// <summary>
    /// Name of the variable storing the RSA private key password. If the key is not encrypted, this value should not be set.
    /// </summary>
    public const string JwtRsaPasswordVariableName = "RINKU_RSA_PASSWORD";

    private const string BASE_URL_VARIABLE_NAME = "RINKU_IDENTITY_BASEURL";
    private const string INITIAL_USER_EMAIL = "RINKU_INITIAL_USER_EMAIL";
    private const string INITIAL_USER_PASSWORD = "RINKU_INITIAL_USER_PASSWORD";

    /// <summary>
    /// Returns an env variable of the required name. If the variable is not set, throws <see cref="InvalidOperationException"/>>
    /// </summary>
    /// <param name="variableName">Exact name of the variable to get.</param>
    /// <returns>Value of the variable</returns>
    /// <exception cref="InvalidOperationException">Thrown when no variable of the given name was found.</exception>
    public static string GetRequiredVariable(string variableName)
    {
        var variableValue = Environment.GetEnvironmentVariable(variableName);
        if (variableValue is null)
        {
            throw new InvalidOperationException($"Environmental variable {variableName} is not set");
        }
        return variableValue;
    }

    public static bool IsDefaultUserProvided(out string email, out string password)
    {
        email = GetOptionalVariable(INITIAL_USER_EMAIL);
        password = GetOptionalVariable(INITIAL_USER_PASSWORD);

        return !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password);
    }

    /// <summary>
    /// Returns an env variable of the required name or the <paramref name="defaultValue"/> if not set.
    /// </summary>
    /// <param name="variableName">Exact name of the variable.</param>
    /// <param name="defaultValue">Fallback value to be used when variable not set. Empty by default.</param>
    /// <returns>Value of the variable or the fallback <paramref name="defaultValue"/> when not set.</returns>
    public static string GetOptionalVariable(string variableName, string defaultValue = "") => Environment.GetEnvironmentVariable(variableName) ?? defaultValue;

    private static readonly Lazy<string> baseUrl = new Lazy<string>(() => GetRequiredVariable(BASE_URL_VARIABLE_NAME).TrimEnd('/') + "/");
    /// <summary>
    /// Returns the base url of the application (ie http://rinkudesu-identity:5500/) - the trailing / is always set.
    /// </summary>
    [SuppressMessage("Design", "CA1055:URI-like return values should not be strings")]
    [SuppressMessage("Design", "CA1024:Use properties where appropriate")]
    public static string GetBaseUrl() => baseUrl.Value;
}
