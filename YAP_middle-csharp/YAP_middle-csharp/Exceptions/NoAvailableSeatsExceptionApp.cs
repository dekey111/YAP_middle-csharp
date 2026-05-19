namespace YAP_middle_csharp.Exceptions
{
    public class NoAvailableSeatsExceptionApp(string message)
        : BaseApiException(message, StatusCodes.Status409Conflict, "No available seats for this event");
}
