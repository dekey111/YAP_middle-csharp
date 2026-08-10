using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using YAP_middle_csharp_Auth.Domain.Models;

namespace YAP_middle_csharp_Auth.Application.Interfaces.IServices
{
    public interface IUserContextService
    {
        Guid GetCurrentUserId(ClaimsPrincipal user);
        UserRoleEnum GetCurrentUserRole(ClaimsPrincipal user);
    }
}
