namespace YAP_middle_csharp.Exceptions
{
    public class NotFoundExceptionApp(string message)
            : BaseApiException(message, StatusCodes.Status404NotFound, "Resource not found");
}
