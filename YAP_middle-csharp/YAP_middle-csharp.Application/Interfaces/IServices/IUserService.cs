using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp.Domain.Models;

namespace YAP_middle_csharp.Application.Interfaces.IServices
{
    public interface IUserService
    {
        Task RegisterAsync(string login, string password, UserRoleEnum role);
        Task<string> LoginAsync(string login, string password);
    }
}
