using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using YAP_middle_csharp.Application.Interfaces.IServices;
using YAP_middle_csharp.Domain.Exceptions;
using YAP_middle_csharp.Domain.Models;

namespace YAP_middle_csharp.Application.Services
{
    public class UserContextService : IUserContextService
    {
        public Guid GetCurrentUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user?.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var currentUserId))
                throw new UnauthorizedOperationException();
            

            return currentUserId;
        }

        public UserRoleEnum GetCurrentUserRole(ClaimsPrincipal user)
        {
            var roleClaim = user?.FindFirst(ClaimTypes.Role)?.Value ?? user?.FindFirst("role")?.Value;

            if (Enum.TryParse<UserRoleEnum>(roleClaim, true, out var role))
                return role;
            

            return UserRoleEnum.User;
        }
    }
}
