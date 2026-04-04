using System.ComponentModel.DataAnnotations;

namespace YAP_middle_csharp.Models
{
    public class EventModel
    {
        [Required(ErrorMessage = "Адрес обязателен для заполнения")]
        public required int id { get; set; }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "Наименование должно быть от 2 до 100 символов")]
        public required string Title { get; set; }
        public string Description { get; set; } = string.Empty;

        [Range(typeof(DateTime), "2010-01-01", "2030-12-31", ErrorMessage = "Некорректная дата")]
        public required DateTime StartAt { get; set; }

        [Range(typeof(DateTime), "2010-01-01", "2030-12-31", ErrorMessage = "Некорректная дата")]
        public required DateTime EndAt { get; set; }
    }
}
