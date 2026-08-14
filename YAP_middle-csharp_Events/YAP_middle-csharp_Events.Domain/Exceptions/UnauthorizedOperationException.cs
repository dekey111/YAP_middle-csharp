using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp_Events.Domain.Exceptions
{
    public class UnauthorizedOperationException()
        : BaseApiException("Недостаточно прав для выполнение операции!", 403, "Access Denied");
}
