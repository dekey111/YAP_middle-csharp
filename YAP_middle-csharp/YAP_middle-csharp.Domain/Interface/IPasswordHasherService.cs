
using YAP_middle_csharp.Domain.Models;

namespace YAP_middle_csharp.Domain.Interface
{
    public interface IPasswordHasherService
    {
        string HashPassword(string password);
        bool CheckPassword(UserModel user, string password);
    }
}
