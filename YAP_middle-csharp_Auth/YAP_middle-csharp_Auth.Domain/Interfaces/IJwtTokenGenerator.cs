using YAP_middle_csharp_Auth.Domain.Models;

namespace YAP_middle_csharp_Auth.Domain.Interface
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(UserModel user);
    }
}
