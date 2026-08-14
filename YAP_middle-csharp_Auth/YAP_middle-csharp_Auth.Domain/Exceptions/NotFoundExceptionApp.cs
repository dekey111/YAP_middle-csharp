namespace YAP_middle_csharp_Auth.Domain.Exceptions
{
    /// <summary>
    /// Кастомный экспешн для обработки не найденных сущностей
    /// </summary>
    /// <param name="message">Принимает сообщение из сущности ошибки</param>
    public class NotFoundExceptionApp(string message)
            : BaseApiException(message, 404, "Resource not found");
}
