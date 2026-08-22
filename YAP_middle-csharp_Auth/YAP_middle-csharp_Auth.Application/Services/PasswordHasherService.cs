using Microsoft.AspNetCore.Identity;
using YAP_middle_csharp_Auth.Domain.Interfaces;
using YAP_middle_csharp_Auth.Domain.Models;

namespace YAP_middle_csharp_Auth.Application.Services
{
    public class PasswordHasherService : IPasswordHasherService
    {
        private readonly IPasswordHasher<UserModel> _hasher;

        public PasswordHasherService(IPasswordHasher<UserModel> hasher)
        {
            _hasher = hasher;
        }


        public bool CheckPassword(UserModel user, string password, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var checkpassword = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return checkpassword != PasswordVerificationResult.Failed;
        }

        public string HashPassword(string password, CancellationToken cancellationToken = default)
        {

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            cancellationToken.ThrowIfCancellationRequested();

            return _hasher.HashPassword(null!, password);
        }
    }

}
