using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp_Booking.Domain.Exceptions
{
    public class BookingLimitExceededException : BaseApiException
    {
        public int Limit { get; }

        public BookingLimitExceededException(int limit)
            : base($"Превышет максимальный лимит активных бронирований (Лимит: {limit})", 409, "Booking Limit Exceeded")
        {
            Limit = limit;
        }
    }
}
