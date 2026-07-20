using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp.Domain.Models;

namespace YAP_middle_csharp.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(UserModel user);
    }
}
