using System.ComponentModel.DataAnnotations;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

public class ChangeEmailDto
{
    [Required]
    public string Email { get; set; } = string.Empty;
}
