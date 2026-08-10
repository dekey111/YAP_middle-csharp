using YAP_middle_csharp_Auth.Application.Interfaces;
using YAP_middle_csharp_Auth.Application.Models;

namespace YAP_middle_csharp_Auth.Application.Validators
{
    /// <summary>
    /// Класс для проверки валидации полей Event
    /// </summary>
    public class LoginValidator : IValidator<LoginRequest>
    {

        /// <summary>
        /// Метод нахождения всех ошибок валидации
        /// </summary>
        /// <param name="item">Принимает модель проверки</param>
        /// <returns>Возвращает список найденных ошибок</returns>
        public IEnumerable<string> GetErrors(LoginRequest item)
        {
            if (item == null)
            {
                yield return "Ошибка при получение данных авторизации!";
                yield break;
            }

            if (string.IsNullOrWhiteSpace(item.Login))
                yield return "Поле 'Логин' должен быть не менее 3 и до 50 символов";

            if (string.IsNullOrWhiteSpace(item.Password))
                yield return "Поле 'Пароль' должен быть не менее 5 и до 20 символов";
        }

        /// <summary>
        /// Метод проверки есть ли хоть одна ошибка
        /// </summary>
        /// <param name="item">Принимает модель проверки</param>
        /// <returns>Возвращает true - в случае если ошибки есть. False - в случае отсутствия ошибок</returns>
        public bool IsValid(LoginRequest item)
        {
            return !GetErrors(item).Any();
        }
    }
}
