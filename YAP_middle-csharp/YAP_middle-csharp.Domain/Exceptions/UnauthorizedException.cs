using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp.Domain.Exceptions
{
    public class UnauthorizedException() : BaseApiException("Ошибка авторизации", 401, "Unauthorized");
}
