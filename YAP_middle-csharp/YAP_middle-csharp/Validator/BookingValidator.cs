using YAP_middle_csharp.Interfaces;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Validator
{
    public class BookingValidator : IValidator<BookingModel>
    {
        /// <summary>
        /// Метод нахождения всех ошибок валидации
        /// </summary>
        /// <param name="item">Принимает модель Booking</param>
        /// <returns>Возвращает список найденных ошибок</returns>
        public IEnumerable<string> GetErrors(BookingModel item)
        {
            if (item == null)
            {
                yield return "Модель события не передана (null)!";
                yield break;
            }

            if (item.EventId == Guid.Empty)
                yield return "Идентификатор события (EventId) должен быть указан!";

            if (item.EventId == Guid.Empty)
                yield return "Идентификатор события (EventId) должен быть указан!";

            if (item.CreatedAt == default)
                yield return "Поле 'Дата создания' не может быть пустым!";

            if (item.ProcessedAt == default)
                yield return "Поле 'Дата процесса' не может быть пустым!";

            if (item.ProcessedAt != default && item.CreatedAt != default && item.CreatedAt > item.ProcessedAt)
                yield return "Дата процесса не может быть раньше даты создания!";
        }

        /// <summary>
        /// Метод проверки есть ли хоть одна ошибка
        /// </summary>
        /// <param name="item">Принимает модель Booking</param>
        /// <returns>Возвращает true - в случае если ошибки есть. False - в случае отсутствия ошибок</returns>
        public bool IsValid(BookingModel item)
        {
            return !GetErrors(item).Any();
        }
    }
}
