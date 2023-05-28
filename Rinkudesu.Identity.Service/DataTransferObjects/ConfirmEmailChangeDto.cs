using System.ComponentModel.DataAnnotations;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

public class ConfirmEmailChangeDto
{
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public string Token { get; set; } = string.Empty;
    [Required]
    public string NewEmail { get; set; } = string.Empty;
}
