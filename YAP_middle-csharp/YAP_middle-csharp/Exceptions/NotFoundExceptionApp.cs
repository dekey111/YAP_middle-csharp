namespace YAP_middle_csharp.Exceptions
{
    public class NotFoundExceptionApp(string message)
            : BaseApiException(message, StatusCodes.Status400BadRequest, "Resource not found");
}
