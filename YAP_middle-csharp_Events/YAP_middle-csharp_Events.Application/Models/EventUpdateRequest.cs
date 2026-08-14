using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using YAP_middle_csharp_Events.Domain.Models;

namespace YAP_middle_csharp_Events.Application.Models
{
    /// <summary>
    /// Кастомная модель событий для обновления/удаления модели
    /// </summary>
    public class EventUpdateRequest
    {
        public EventUpdateRequest() { }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "Наименование должно быть от 2 до 100 символов")]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Range(1, 250, ErrorMessage = "Количество мест должно быть от 1 до 250")]
        public int TotalSeats { get; set; }

        [Range(typeof(DateTime), "2010-01-01", "2030-12-31", ErrorMessage = "Некорректная дата")]
        public DateTime StartAt { get; set; }

        [Range(typeof(DateTime), "2010-01-01", "2030-12-31", ErrorMessage = "Некорректная дата")]
        public DateTime EndAt { get; set; }

        public EventUpdateRequest(EventModel eventModel)
        {
            Title = eventModel.Title;
            Description = eventModel.Description;
            TotalSeats = eventModel.TotalSeats;
            StartAt = eventModel.StartAt;
            EndAt = eventModel.EndAt;
        }

    }
}
