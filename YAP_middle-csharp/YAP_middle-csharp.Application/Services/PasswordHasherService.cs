using Microsoft.AspNetCore.Identity;
using YAP_middle_csharp.Domain.Interface;
using YAP_middle_csharp.Domain.Models;

namespace YAP_middle_csharp.Application.Services
{
    public class PasswordHasherService : IPasswordHasherService
    {
        private readonly IPasswordHasher<UserModel> _hasher;

        public PasswordHasherService(IPasswordHasher<UserModel> hasher)
        {
            _hasher = hasher;
        }


        public bool CheckPassword(UserModel user, string password)
        {
            var checkpassword = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return checkpassword != PasswordVerificationResult.Failed;
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            return _hasher.HashPassword(null!, password);
        }
    }
}
