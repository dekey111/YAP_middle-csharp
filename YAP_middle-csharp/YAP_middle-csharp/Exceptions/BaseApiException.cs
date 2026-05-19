namespace YAP_middle_csharp.Exceptions
{
    public abstract class BaseApiException(string message, int statusCode, string title) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
        public string Title { get; } = title;
    }
}
