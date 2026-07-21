using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using YAP_middle_csharp.Application.Interfaces.IServices;

namespace YAP_middle_csharp.Application.Services
{
    public class PasswordHasherService : IPasswordHasherService
    {
        public bool CheckPassword(string password, string hasPassword)
        {
            var findHasPassowrd = HasPassword(hasPassword);
            return string.Equals(password, findHasPassowrd, StringComparison.OrdinalIgnoreCase);
        }

        public string HasPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }
}
