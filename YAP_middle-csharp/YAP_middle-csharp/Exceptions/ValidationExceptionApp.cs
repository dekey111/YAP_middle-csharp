namespace YAP_middle_csharp.Exceptions
{
    /// <summary>
    /// Кастомный экспешн для обработки валидационных ошибок
    /// </summary>
    /// <param name="message">Принимает сообщение из сущности ошибки</param>
    public class ValidationExceptionApp(string message)
            : BaseApiException(message, StatusCodes.Status400BadRequest, "Validation Error");
}
