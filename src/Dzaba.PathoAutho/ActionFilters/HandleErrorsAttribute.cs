using Dzaba.PathoAutho.Lib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dzaba.PathoAutho.ActionFilters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class HandleErrorsAttribute : ActionFilterAttribute, IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var container = context.HttpContext.RequestServices;
        var logger = container.GetRequiredService<ILogger<HandleErrorsAttribute>>();

        if (context.Exception is ModelStateException msEx)
        {
            logger.LogWarning(msEx, "Model state error");

            var modelState = new ModelStateDictionary();
            foreach (var error in msEx.Errors)
            {
                modelState.AddModelError(error.Key, error.Value);
            }

            context.Result = new BadRequestObjectResult(modelState);
            return;
        }

        if (context.Exception is HttpResponseException httpEx)
        {
            logger.LogWarning(httpEx, "HTTP response error");

            context.Result = new ObjectResult(httpEx.Message)
            {
                StatusCode = (int)httpEx.StatusCode
            };
            return;
        }
    }
}
