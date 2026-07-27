using System.Security.Claims;
using YAP_middle_csharp.Domain.Models;

namespace YAP_middle_csharp.Application.Interfaces.IServices
{
    public interface IUserContextService
    {
        Guid GetCurrentUserId(ClaimsPrincipal user);
        UserRoleEnum GetCurrentUserRole(ClaimsPrincipal user);
    }
}
