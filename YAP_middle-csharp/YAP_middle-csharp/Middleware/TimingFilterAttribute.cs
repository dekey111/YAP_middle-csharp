using Microsoft.AspNetCore.Mvc.Filters;

namespace YAP_middle_csharp.Middleware
{
    public class TimingFilterAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            await next();

            stopwatch.Stop();
            Console.WriteLine($"[TimingFilterAttribute] Execution time: {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
