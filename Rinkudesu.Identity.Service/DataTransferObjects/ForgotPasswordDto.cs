using System.ComponentModel.DataAnnotations;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

public class ForgotPasswordDto
{
    [Required]
    public string Email { get; set; } = string.Empty;
}
