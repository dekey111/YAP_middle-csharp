using YAP_middle_csharp.Interfaces;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Validator
{
    /// <summary>
    /// Класс для проверки валидации полей Event
    /// </summary>
    public class EventValidator : IValidator<EventModel>
    {

        /// <summary>
        /// Метод нахождения всех ошибок валидации
        /// </summary>
        /// <param name="item">Принимает модель Event</param>
        /// <returns>Возвращает список найденных ошибок</returns>
        public IEnumerable<string> GetErrors(EventModel item)
        {
            if (item == null)
            {
                yield return "Модель события не передана (null)!";
                yield break;
            }

            if (string.IsNullOrWhiteSpace(item.Title))
                yield return "Поле 'заголовок' не может быть пустым!";
            
            if (item.StartAt == default)
                yield return "Поле 'дата начала' не может быть пустым!";

            if (item.EndAt == default)
                yield return "Поле 'дата окончания' не может быть пустым!";

            if (item.StartAt != default && item.EndAt != default && item.StartAt > item.EndAt)
                yield return "Дата окончания не может быть раньше даты начала!";
        }

        /// <summary>
        /// Метод проверки есть ли хоть одна ошибка
        /// </summary>
        /// <param name="item">Принимает модель Event</param>
        /// <returns>Возвращает true - в случае если ошибки есть. False - в случае отсутствия ошибок</returns>
        public bool IsValid(EventModel item)
        {
            return !GetErrors(item).Any();
        }
    }
}
