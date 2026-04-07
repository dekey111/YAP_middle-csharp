using Microsoft.AspNetCore.Mvc.Filters;

namespace YAP_middle_csharp.Middleware
{
    public class LogActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            Console.WriteLine($"[LogActionFilter] Executing: {context.ActionDescriptor.DisplayName}");
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            Console.WriteLine($"[LogActionFilter] Executed: {context.ActionDescriptor.DisplayName}");
        }
    }
}
