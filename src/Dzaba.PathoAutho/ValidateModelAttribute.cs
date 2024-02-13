using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dzaba.PathoAutho;

[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
public sealed class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid == false)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }
}
