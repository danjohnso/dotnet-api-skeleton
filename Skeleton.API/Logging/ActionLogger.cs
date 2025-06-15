using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Skeleton.API.Logging
{
    /// <summary>
    /// Action logger for controllers to capture input and output parameters and objects
    /// </summary>
    /// <remarks>
    /// CTOR
    /// </remarks>
    /// <param name="logger"></param>
    public class ActionLogger(ILogger<ActionLogger> logger) : IAsyncResultFilter, IAsyncActionFilter
    {
        private readonly ILogger<ActionLogger> _logger = logger;

        /// <summary>
        /// IAsyncActionFilter, logging of action arguments for POST, PUT, and PATCH calls
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            switch (context.HttpContext.Request.Method.ToUpper())
            {
                case "POST":
                case "PUT":
                case "PATCH":
                    _logger.LogInformation("Starting {URL} with {@Request}", context.HttpContext.Request.Path.ToString(), context.ActionArguments);
                    break;
            }

            await next();
        }

        /// <summary>
        /// IAsyncResultFilter, logging of response ObjectResults
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult preResponse)
            {
                _logger.LogInformation("Completing {URL} with {ResponseCode} - {@Response}", context.HttpContext.Request.Path.ToString(), preResponse.StatusCode, preResponse.Value);
            }

            await next();
        }
    }
}
