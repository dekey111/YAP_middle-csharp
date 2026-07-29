using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp.Domain.Models;

namespace YAP_middle_csharp.Application.Interfaces.IRepositories
{
    public interface IUserRepository
    {
        Task<UserModel?> FindByIdAsync(Guid id);
        Task<UserModel?> FindByLoginAsync(string login);
        Task CreateAsync(UserModel user);
    }
}
