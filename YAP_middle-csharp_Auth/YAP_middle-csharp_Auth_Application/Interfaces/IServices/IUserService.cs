using YAP_middle_csharp_Auth.Domain.Models;

namespace YAP_middle_csharp_Auth.Application.Interfaces.IServices
{
    public interface IUserService
    {
        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <param name="role">Роль пользователя</param>
        Task RegisterAsync(string login, string password, UserRoleEnum role);

        /// <summary>
        /// Авторизация пользователя
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns>Возвращает сгенерированный JWT токен</returns>
        Task<string> LoginAsync(string login, string password);
    }
}
