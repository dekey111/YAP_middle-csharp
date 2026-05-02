using System.ComponentModel.DataAnnotations;

namespace YAP_middle_csharp.Models
{
    public class EventRequest
    {
        public EventRequest() { }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "Наименование должно быть от 2 до 100 символов")]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Range(typeof(DateTime), "2010-01-01", "2030-12-31", ErrorMessage = "Некорректная дата")]
        public required DateTime StartAt { get; set; }

        [Range(typeof(DateTime), "2010-01-01", "2030-12-31", ErrorMessage = "Некорректная дата")]
        public required DateTime EndAt { get; set; }

        public EventRequest(EventModel eventModel)
        {
            Title = eventModel.Title;
            Description = eventModel.Description;
            StartAt = eventModel.StartAt;
            EndAt = eventModel.EndAt;
        }
    }
}
