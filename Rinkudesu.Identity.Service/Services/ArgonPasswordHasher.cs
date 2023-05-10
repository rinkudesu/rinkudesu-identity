using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Rinkudesu.Identity.Service.Models;
using Rinkudesu.Identity.Service.Utilities;

namespace Rinkudesu.Identity.Service.Services;

/// <summary>
/// Implements the <see cref="IPasswordHasher{TUser}"/> interface for the <see cref="User"/> class using the Argon2id hashing algorithm.
/// </summary>
public class ArgonPasswordHasher : IPasswordHasher<User>
{
    //this value can be static as it will basically never change for the entire life of the application
    private static readonly Lazy<byte[]> argonSecret = new Lazy<byte[]>(ArgonSettingsReader.GetSecret);

    /// <inheritdoc/>
    public string HashPassword(User user, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(128);
        return HashPasswordWithSalt(password, salt);
    }

    /// <inheritdoc/>
    public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
            return PasswordVerificationResult.Failed;

        var argonStringSplit = hashedPassword.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (argonStringSplit.Length != 5)
            return PasswordVerificationResult.Failed;

        var salt = argonStringSplit[^2];
        var hashedProvidedPassword = HashPasswordWithSalt(providedPassword, Convert.FromBase64String(salt));

        if (CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(hashedProvidedPassword), Encoding.UTF8.GetBytes(hashedPassword)))
            return PasswordVerificationResult.Success;

        return PasswordVerificationResult.Failed;
    }

    private static string HashPasswordWithSalt(string password, byte[] salt)
    {
        //todo: add a way to change those values from config file and then automatically rehash credentials on next login
        //those parameters are not configurable as changing them on a live database without a way of migrating users would probably collapse the universe
        using var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Iterations = 3,
            DegreeOfParallelism = 4,
            MemorySize = 256 * 1024,
            Salt = salt,
            KnownSecret = argonSecret.Value,
        };
        var hash = argon.GetBytes(256);
        return GetFullString(argon, hash);
    }

    // v=19 is hard-coded here as this library has support only for this one version and there's no way of changing/selecting that atm.
    // see https://github.com/kmaragon/Konscious.Security.Cryptography/issues/30
    private static string GetFullString(Argon2id argon, byte[] hash) => $"$argon2id$v=19$m={argon.MemorySize},t={argon.Iterations},p={argon.DegreeOfParallelism}${Convert.ToBase64String(argon.Salt)}${Convert.ToBase64String(hash)}";
}
