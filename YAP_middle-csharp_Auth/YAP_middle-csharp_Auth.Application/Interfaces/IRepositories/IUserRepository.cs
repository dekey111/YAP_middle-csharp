using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp_Auth.Domain.Models;

namespace YAP_middle_csharp_Auth.Application.Interfaces.IRepositories
{
    /// <summary>
    /// Репозиторий по работе с пользователями
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Поиск пользователя по Уникальному идентификатору
        /// </summary>
        /// <param name="id">УИ</param>
        /// <returns>Возвращает найденную сущность, нибо 404-NotFound</returns>
        Task<UserModel> FindByIdAsync(Guid id);

        /// <summary>
        /// Поиск пользователя по логину
        /// </summary>
        /// <param name="login">логин пользователя</param>
        /// <returns>Возвращает найденную сущность, нибо 404-NotFound</returns>
        Task<UserModel?> FindByLoginAsync(string login);


        /// <summary>
        /// Создание нового пользователя
        /// </summary>
        /// <param name="user">Сущность пользователя</param>
        Task CreateAsync(UserModel user);
    }
}
