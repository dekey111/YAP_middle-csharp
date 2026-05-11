using System.ComponentModel.DataAnnotations;

namespace YAP_middle_csharp.Models
{
    public class EventModel
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string Description { get; set; } = string.Empty;

        private DateTimeOffset _startAt;
        public required DateTime StartAt
        {
            get => _startAt.UtcDateTime;
            set => _startAt = value.Kind == DateTimeKind.Unspecified
                          ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                          : value.ToUniversalTime();
        }

        private DateTimeOffset _endAt;
        public required DateTime EndAt
        {
            get => _endAt.UtcDateTime;
            set => _endAt = value.Kind == DateTimeKind.Unspecified 
                          ? DateTime.SpecifyKind(value, DateTimeKind.Utc) 
                          : value.ToUniversalTime(); 
        }
    }
}
