using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp.Contracts.EventModels
{
    public class EventContract
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int AvailableSeats { get; set; }
    }
}
