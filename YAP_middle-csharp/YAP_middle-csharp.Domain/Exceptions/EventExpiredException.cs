using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharpю.Domain.Exceptions;

namespace YAP_middle_csharp.Domain.Exceptions
{
    public class EventExpiredException() 
        : BaseApiException("Срок регистрации или проведения события уже истек", 400, "Event Registration Expired");
}
