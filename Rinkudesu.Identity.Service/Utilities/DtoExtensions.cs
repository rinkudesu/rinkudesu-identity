using System.Diagnostics.CodeAnalysis;
using Rinkudesu.Identity.Service.DataTransferObjects;
using Rinkudesu.Identity.Service.Models;
#pragma warning disable CS1591

namespace Rinkudesu.Identity.Service.Utilities;

public static class DtoExtensions
{
    [ExcludeFromCodeCoverage]
    public static User MakeUser(this AdminAccountCreateDto dto)
        => User.CreateWithEmail(dto.Email);
}
