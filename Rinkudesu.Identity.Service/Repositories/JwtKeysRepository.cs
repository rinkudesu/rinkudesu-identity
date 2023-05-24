using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Rinkudesu.Identity.Service.Common.Utilities;
using Rinkudesu.Identity.Service.Utilities;

namespace Rinkudesu.Identity.Service.Repositories;

/// <summary>
/// Allows for retrieval of RSA keys used for JWT signing and converting them to a desired format.
/// </summary>
[ExcludeFromCodeCoverage]
[SuppressMessage("Design", "CA1024:Use properties where appropriate")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class JwtKeysRepository
{
    private const string CERT_DIR_NAME = "JWK";

    private readonly Lazy<RSA> rsaKey = new Lazy<RSA>(() => {
        var rsa = RSA.Create();
        var fileName = Directory.EnumerateFiles(CERT_DIR_NAME).FirstOrDefault(f => f.EndsWith(".pem", StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrEmpty(fileName))
            throw new InvalidOperationException($"JWK RSA keys need to be stored in the {CERT_DIR_NAME} folder.");
        var rsaPassword = EnvironmentalVariablesReader.GetOptionalVariable(EnvironmentalVariablesReader.JwtRsaPasswordVariableName);
        var rsaInput = File.ReadAllText(fileName);
        if (string.IsNullOrEmpty(rsaPassword))
            rsa.ImportFromPem(rsaInput);
        else
            rsa.ImportFromEncryptedPem(rsaInput, rsaPassword);
        return rsa;
    });

    /// <summary>
    /// Returns a complete RSA key pair value.
    /// </summary>
    public RSA GetRsaKeyPair() => rsaKey.Value;
    /// <summary>
    /// Returns the public key in a <see cref="RsaSecurityKey"/> format.
    /// </summary>
    public RsaSecurityKey GetRsaAsSecurityKey() => new RsaSecurityKey(GetRsaKeyPair().ExportParameters(false));
    /// <summary>
    /// Returns the public key as a json web key.
    /// </summary>
    public JsonWebKey GetRsaAsJsonWebKey() => JsonWebKeyConverter.ConvertFromRSASecurityKey(GetRsaAsSecurityKey());
    //todo: this assumes that the RSA key pair was generated like so: "openssl genpkey -algorithm RSA -out private_key.pem -aes256" -- it should somehow select it based on the key itself
    /// <summary>
    /// Returns the public RSA key as a signing credential
    /// </summary>
    public SigningCredentials GetRsaAsSigningCredentials() => new SigningCredentials(GetRsaAsSecurityKey(), SecurityAlgorithms.RsaSsaPssSha256);
}
