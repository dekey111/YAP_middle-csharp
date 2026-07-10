namespace YAP_middle_csharpю.Domain.Exceptions
{

    /// <summary>
    /// Базовый кастомный эксепшн
    /// </summary>
    /// <param name="message">Образует маленькие кастомы в общий формат</param>
    public abstract class BaseApiException(string message, int statusCode, string title) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
        public string Title { get; } = title;
    }
}
