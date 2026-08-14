using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp_Booking.Domain.Models
{
    /// <summary>
    /// Базовая модель статусов бронирования из БД
    /// </summary>
    public enum BookingStatusEnum
    {
        Pending,
        Confirmed,
        Rejected,
        Cancelled
    }
}
