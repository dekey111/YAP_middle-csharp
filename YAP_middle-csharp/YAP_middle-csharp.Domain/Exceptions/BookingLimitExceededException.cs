using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharpю.Domain.Exceptions;

namespace YAP_middle_csharp.Domain.Exceptions
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
