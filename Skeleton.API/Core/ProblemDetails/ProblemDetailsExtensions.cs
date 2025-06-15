using FluentValidation.Results;
using Skeleton.Core;
using Skeleton.Core.Enums;
using System.Diagnostics;

namespace Skeleton.API.Core.ProblemDetails
{
    public static class ProblemDetailsExtensions
    {
        public static Microsoft.AspNetCore.Mvc.ProblemDetails ToProblemDetails(this ValidationResult result, HttpContext httpContext)
        {
            Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = new()
            {
                Type = ProblemDetailsConstants.Status400Uri,
                Title = "Bad Request",
                Status = StatusCodes.Status400BadRequest,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
            };

            if (result.Errors.Count > 0)
            {
                Dictionary<string, List<Problem>> errors = [];
                foreach (ValidationFailure failure in result.Errors)
                {
                    if (errors.TryGetValue(failure.PropertyName, out List<Problem>? value))
                    {
                        value.Add(new Problem(failure.ErrorCode, failure.ErrorMessage));
                    }
                    else
                    {
                        errors.Add(failure.PropertyName, [new(failure.ErrorCode, failure.ErrorMessage)]);
                    }
                }

                problemDetails.Extensions.Add(ProblemDetailsConstants.ErrorsKey, errors);
            }

            string? traceId = httpContext.GetTraceId();
            if (traceId != null)
            {
                problemDetails.Extensions.Add(ProblemDetailsConstants.TraceIdKey, traceId);
            }

            return problemDetails;
        }

        public static Microsoft.AspNetCore.Mvc.ProblemDetails ToProblemDetails<T>(this Result<T> result, HttpContext httpContext)
        {
            Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = new()
            {
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
            };

            switch (result.StatusType)
            {
                case ResultStatusType.Invalid:
                    problemDetails.Type = ProblemDetailsConstants.Status400Uri;
                    problemDetails.Title = "Bad Request";
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    break;
                case ResultStatusType.NotFound:
                    problemDetails.Type = ProblemDetailsConstants.Status404Uri;
                    problemDetails.Title = "Not Found";
                    problemDetails.Status = StatusCodes.Status404NotFound;
                    break;
                case ResultStatusType.Conflict:
                    problemDetails.Type = ProblemDetailsConstants.Status409Uri;
                    problemDetails.Title = "Conflict";
                    problemDetails.Status = StatusCodes.Status409Conflict;
                    break;
                case ResultStatusType.Error:
                    problemDetails.Type = ProblemDetailsConstants.Status500Uri;
                    problemDetails.Title = "Unexpected Error";
                    problemDetails.Status = StatusCodes.Status500InternalServerError;
                    break;
            }

            if (result.Problems.Count > 0)
            {
                problemDetails.Extensions.Add(ProblemDetailsConstants.ErrorsKey, result.Problems);
            }

            string? traceId = httpContext.GetTraceId();
            if (traceId != null)
            {
                problemDetails.Extensions.Add(ProblemDetailsConstants.TraceIdKey, traceId);
            }

            return problemDetails;
        }

        public static string? GetTraceId(this HttpContext context)
        {
            // https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/Infrastructure/DefaultProblemDetailsFactory.cs#L90
            return Activity.Current?.Id ?? context?.TraceIdentifier;
        }
    }
}
