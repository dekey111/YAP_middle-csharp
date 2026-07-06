namespace YAP_middle_csharp.Exceptions
{
    /// <summary>
    /// Кастомный экспешн для обработки свободных мест при бронировании
    /// </summary>
    /// <param name="message">Принимает сообщение из сущности ошибки</param>
    public class NoAvailableSeatsExceptionApp(string message)
        : BaseApiException(message, 409, "No available seats for this event");
}
