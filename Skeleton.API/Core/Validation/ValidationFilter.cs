using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Skeleton.API.Core.ProblemDetails;

namespace Skeleton.API.Core.Validation
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ValidationFilter<T> : ActionFilterAttribute where T : class
    {
        public string ArgumentName { get; set; } = "request";

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            IValidator<T> validator = context.HttpContext.RequestServices.GetRequiredService<IValidator<T>>();

            T? request = context.ActionArguments[ArgumentName] as T;
            if (request is not null)
            {
                ValidationResult validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    context.Result = new BadRequestObjectResult(validationResult.ToProblemDetails(context.HttpContext));
                    return;
                }
            }

            await next();
        }
    }
}
