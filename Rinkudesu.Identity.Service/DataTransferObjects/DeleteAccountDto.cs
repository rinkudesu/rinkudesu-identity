using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

[ExcludeFromCodeCoverage]
public class DeleteAccountDto
{
    [JsonPropertyName("password")]
    public string Password { get; set; }
}
