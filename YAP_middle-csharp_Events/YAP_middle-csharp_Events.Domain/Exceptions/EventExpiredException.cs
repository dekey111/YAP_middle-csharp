using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp_Events.Domain.Exceptions
{
    public class EventExpiredException()
        : BaseApiException("Срок регистрации или проведения события уже истек", 400, "Event Registration Expired");
}
