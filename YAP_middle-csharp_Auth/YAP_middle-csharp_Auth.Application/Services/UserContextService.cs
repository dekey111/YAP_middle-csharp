using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using YAP_middle_csharp_Auth.Application.Interfaces.IServices;
using YAP_middle_csharp_Auth.Domain.Exceptions;
using YAP_middle_csharp_Auth.Domain.Models;

namespace YAP_middle_csharp_Auth.Application.Services
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

            throw new UnauthorizedOperationException();
        }
    }

}
