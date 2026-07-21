using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp.Domain.Models
{
    public class UserModel
    {
        public Guid Id { get; private set; }
        public string Login { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public UserRoleEnum UserRole { get; private set; }

        public UserModel(string login, string passwordHash, UserRoleEnum userRole)
        {
            Id = Guid.NewGuid();
            Login = login;
            PasswordHash = passwordHash;
            UserRole = userRole;
        }
    }
}
