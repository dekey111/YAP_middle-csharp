using YAP_middle_csharp_Auth.Domain.Models;

namespace YAP_middle_csharp_Auth.Domain.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(UserModel user, CancellationToken cancellationToken = default);
    }
}
