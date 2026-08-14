using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp_Booking.Domain.Exceptions
{
    public class UnauthorizedException() : BaseApiException("Ошибка авторизации", 401, "Unauthorized");
}
