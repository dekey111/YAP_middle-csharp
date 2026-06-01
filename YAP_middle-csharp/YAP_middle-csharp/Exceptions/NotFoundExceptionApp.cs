namespace YAP_middle_csharp.Exceptions
{
    /// <summary>
    /// Кастомный экспешн для обработки не найденных сущностей
    /// </summary>
    /// <param name="message">Принимает сообщение из сущности ошибки</param>
    public class NotFoundExceptionApp(string message)
            : BaseApiException(message, StatusCodes.Status404NotFound, "Resource not found");
}
