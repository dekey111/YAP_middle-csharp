using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using YAP_middle_csharp.Domain.Exceptions;

namespace YAP_middle_csharp.Middleware
{
    /// <summary>
    /// Кастомный глобальный обработчик ошибок
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Запускаем каждый DI в try catch для глобальной обработки ошибок
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="problemDetailsService"></param>
        public async Task InvokeAsync(HttpContext httpContext, IProblemDetailsService problemDetailsService)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleException(httpContext, ex, problemDetailsService);
            }
        }

        /// <summary>
        /// Метод обработки ошибки, для вывода в единый формат
        /// </summary>
        private async Task HandleException(HttpContext httpContext, Exception ex, IProblemDetailsService problemDetailsService)
        {
            _logger.LogError(
                ex,
                "Unhandled exception. Method={Method}, Path={Path}, RequestId={RequestId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.Request.Headers["x-request-id"]);

            if (httpContext.Response.HasStarted)
            {
                return;
            }

            var (statusCode, title) = ex switch
            {
                BaseApiException domainEx => (domainEx.StatusCode, domainEx.Title),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            var type = statusCode == StatusCodes.Status400BadRequest ? "https://tools.ietf.org/html/rfc9110#section-15.5.1" : ex.GetType().Name;

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = ex.Message,
                Type = type,
                Instance = httpContext.Request.Path
            };

            var errors = new Dictionary<string, string[]>
            {
                { "EventValidation", [ex.Message] }
            };
            problemDetails.Extensions.TryAdd("errors", errors);

            await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                Exception = ex,
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });
        }
    }
}
