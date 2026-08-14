using System;
using System.Collections.Generic;
using System.Text;

namespace YAP_middle_csharp_Events.Domain.Exceptions
{
    /// <summary>
    /// Кастомный экспешн для обработки валидационных ошибок
    /// </summary>
    /// <param name="message">Принимает сообщение из сущности ошибки</param>
    public class ValidationExceptionApp(string message)
            : BaseApiException(message, 400, "Validation Error");
}
