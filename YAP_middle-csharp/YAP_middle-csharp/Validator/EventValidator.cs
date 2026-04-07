using YAP_middle_csharp.Interfaces;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Validator
{
    public class EventValidator : IValidator<EventResponse>
    {
        public IEnumerable<string> GetErrors(EventResponse item)
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
        public bool IsValid(EventResponse item)
        {
            return !GetErrors(item).Any();
        }
    }
}
