using Skeleton.Core;
using Skeleton.API.Core.ProblemDetails;
using System.Text.Json;

namespace Skeleton.API.Core.Core
{
    internal class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
        private readonly ILogger<GlobalExceptionMiddleware> _logger = logger;

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception");

                // reset the response status
                httpContext.Response.Clear();

                Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = new()
                {
                    Type = ProblemDetailsConstants.Status500Uri,
                    Title = "Unexpected Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
                };

                problemDetails.Extensions.Add(ProblemDetailsConstants.ErrorsKey, new Dictionary<string, List<Problem>>()
                {
                    { "", [Problems.UnexpectedError] }
                });

                string? traceId = httpContext.GetTraceId();
                if (traceId != null)
                {
                    problemDetails.Extensions.Add(ProblemDetailsConstants.TraceIdKey, traceId);
                }

                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                httpContext.Response.ContentType = ProblemDetailsConstants.ContentType;

                await httpContext.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
            }
        }
    }
}
