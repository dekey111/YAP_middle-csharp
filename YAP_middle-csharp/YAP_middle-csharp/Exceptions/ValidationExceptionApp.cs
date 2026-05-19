namespace YAP_middle_csharp.Exceptions
{
    public class ValidationExceptionApp(string message)
            : BaseApiException(message, StatusCodes.Status400BadRequest, "Validation Error");
}
