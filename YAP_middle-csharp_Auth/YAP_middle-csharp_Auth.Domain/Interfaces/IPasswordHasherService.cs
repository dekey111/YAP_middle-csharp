using YAP_middle_csharp_Auth.Domain.Models;

namespace YAP_middle_csharp_Auth.Domain.Interfaces
{
    public interface IPasswordHasherService
    {
        string HashPassword(string password);
        bool CheckPassword(UserModel user, string password);
    }
}
