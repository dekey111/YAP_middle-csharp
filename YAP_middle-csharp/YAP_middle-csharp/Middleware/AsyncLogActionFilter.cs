using Microsoft.AspNetCore.Mvc.Filters;

namespace YAP_middle_csharp.Middleware
{
    public class AsyncLogActionFilter(ILogger<AsyncLogActionFilter> logger) : IAsyncActionFilter
    {
        private readonly ILogger<AsyncLogActionFilter> _logger = logger;
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _logger.LogInformation("[AsyncLogActionFilter] Action starting: {ActionName}", context.ActionDescriptor.DisplayName);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await next();

            stopwatch.Stop();

            _logger.LogInformation("[AsyncLogActionFilter] Action completed: {ActionName} in {ElapsedMS}ms", 
                context.ActionDescriptor.DisplayName, stopwatch.ElapsedMilliseconds);
        }
    }
}
