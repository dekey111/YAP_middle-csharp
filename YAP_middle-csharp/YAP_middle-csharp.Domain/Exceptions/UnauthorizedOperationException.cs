using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharpю.Domain.Exceptions;

namespace YAP_middle_csharp.Domain.Exceptions
{
    public class UnauthorizedOperationException()
        : BaseApiException("Недостаточно прав для выполнение операции!", 403, "Access Denied");
}
