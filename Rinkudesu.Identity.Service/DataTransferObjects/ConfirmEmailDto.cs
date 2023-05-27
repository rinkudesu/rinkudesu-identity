using System.ComponentModel.DataAnnotations;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

public class ConfirmEmailDto
{
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public string EmailToken { get; set; } = string.Empty;
}
