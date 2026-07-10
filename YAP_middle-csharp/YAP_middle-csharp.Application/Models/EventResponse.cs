using System.ComponentModel.DataAnnotations;
using YAP_middle_csharp.Domain.Models;

namespace YAP_middle_csharp.Application.Models
{
    /// <summary>
    /// Кастомная модель событий для обновления/удаления модели
    /// </summary>
    public class EventResponse
    {
        public EventResponse() { }
        public Guid Id { get; set; }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "Наименование должно быть от 2 до 100 символов")]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Range(1, 250, ErrorMessage = "Количество мест должно быть от 1 до 250")]
        public int TotalSeats { get; set; }

        [Range(typeof(DateTime), "2010-01-01", "2030-12-31", ErrorMessage = "Некорректная дата")]
        public DateTime StartAt { get; set; }

        [Range(typeof(DateTime), "2010-01-01", "2030-12-31", ErrorMessage = "Некорректная дата")]
        public DateTime EndAt { get; set; }

        public EventResponse(EventModel eventModel)
        {
            Id = eventModel.Id;
            Title = eventModel.Title;
            Description = eventModel.Description;
            TotalSeats = eventModel.TotalSeats;
            StartAt = eventModel.StartAt;
            EndAt = eventModel.EndAt;
        }

    }
}
