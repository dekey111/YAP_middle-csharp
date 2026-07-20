using System.Diagnostics.CodeAnalysis;
using YAP_middle_csharp.Domain.Exceptions;

namespace YAP_middle_csharp.Domain.Models
{
    /// <summary>
    /// Базовая модель бронирований из БД
    /// </summary>
    public class BookingModel
    {
        public required Guid Id { get; set; }
        public required Guid EventId { get; set; }
        public required Guid UserId { get; set; }
        public required BookingStatusEnum Status { get; set; }

        private DateTimeOffset _createdAt; 
        public required DateTime CreatedAt 
        {
            get => _createdAt.UtcDateTime;
            set => _createdAt = value.Kind == DateTimeKind.Unspecified
                                           ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                                           : value.ToUniversalTime();
        }

        private DateTimeOffset? _processedAt;
        public DateTime? ProcessedAt
        {
            get => _processedAt?.UtcDateTime;
            set => _processedAt = value.HasValue 
                ? (value.Value.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) 
                    : value.Value.ToUniversalTime())
                : null;
        }

        public EventModel Event { get; set; } = null!;



        [SetsRequiredMembers]
        private BookingModel()
        {
        }

        [SetsRequiredMembers]
        public BookingModel(Guid eventId, Guid userId)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            Status = BookingStatusEnum.Pending;
            CreatedAt = DateTime.UtcNow;
            UserId = userId;
        }

        public void Cancel()
        {
            if(Status != BookingStatusEnum.Pending)
            {
                throw new ValidationExceptionApp("Бронирование нельзя отменить, потому что оно уже обработано");
            }

            Status = BookingStatusEnum.Cancelled;
            ProcessedAt = DateTime.UtcNow;
        }
    }
}
