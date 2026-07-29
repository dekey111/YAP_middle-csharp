using YAP_middle_csharp.Domain.Models;

namespace YAP_middle_csharp.Domain.Interface
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(UserModel user);
    }
}
