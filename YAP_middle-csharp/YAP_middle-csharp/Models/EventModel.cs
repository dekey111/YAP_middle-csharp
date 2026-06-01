using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;

namespace YAP_middle_csharp.Models
{
    /// <summary>
    /// Базовая модель событий из БД
    /// </summary>
    public class EventModel
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string Description { get; set; } = string.Empty;

        private int _totalSeats;
        private int _availableSeats;
        public required int TotalSeats
        {
            get => _totalSeats;
            set => _totalSeats = value;
        }
        public int AvailableSeats
        {
            get => _availableSeats;
            set => _availableSeats = value;
        }

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

        public List<BookingModel> Booking { get; set; } = null!;

        public bool TryReserveSeats(int count = 1)
        {
            if (count <= 0) return false;
            int current, updated;

            do
            {
                current = _availableSeats;
                if (current < count) return false;
                    updated = current - count;
            }
            while (Interlocked.CompareExchange(ref _availableSeats, updated, current) != current);

            return true;
        }

        public bool ReleaseSeats(int count = 1)
        {
            if (count <= 0) return false;
            int current, updated;
            do
            {
                current = _availableSeats;
                var total = _totalSeats;
                if(current + count > total) return false;
                    updated = current + count;
            }
            while (Interlocked.CompareExchange(ref _availableSeats, updated, current) != current);
            return true;
        }
    }
}
