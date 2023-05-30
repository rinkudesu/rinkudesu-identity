using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

/// <summary>
/// Data requesting account deletion.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteAccountDto
{
    /// <summary>
    /// Password of the user trying to delete the account.
    /// </summary>
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}
