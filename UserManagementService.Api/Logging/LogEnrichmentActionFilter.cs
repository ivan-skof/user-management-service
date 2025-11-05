using Microsoft.AspNetCore.Mvc.Filters;
using Serilog.Context;
using System.Text.Json;

namespace UserManagementService.Api.Logging;

public class LogEnrichmentActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var methodName = context.ActionDescriptor.DisplayName;
        //warning: using context.ActionArguments as is will log sensitive data like passwords to logs
        //var parameters = JsonSerializer.Serialize(context.ActionArguments); 

        // Push into Serilog context for this request
        LogContext.PushProperty("MethodName", methodName);
        //LogContext.PushProperty("Parameters", parameters); 
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Nothing to clean up — Serilog handles context scope automatically
    }
}
