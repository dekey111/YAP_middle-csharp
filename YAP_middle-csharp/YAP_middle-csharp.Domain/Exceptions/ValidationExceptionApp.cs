using YAP_middle_csharpю.Domain.Exceptions;

namespace YAP_middle_csharp.Domain.Exceptions
{
    /// <summary>
    /// Кастомный экспешн для обработки валидационных ошибок
    /// </summary>
    /// <param name="message">Принимает сообщение из сущности ошибки</param>
    public class ValidationExceptionApp(string message)
            : BaseApiException(message, 400, "Validation Error");
}
