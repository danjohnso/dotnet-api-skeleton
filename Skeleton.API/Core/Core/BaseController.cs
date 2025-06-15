using Skeleton.API.Core.ProblemDetails;
using Skeleton.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Skeleton.API.Logging;
using System.Net.Mime;

namespace Skeleton.API.Core.Core
{
    [ApiController]
    [ServiceFilter(typeof(ActionLogger))]
    [Consumes(MediaTypeNames.Application.Json)]
    [Route("v{version:apiVersion}/[controller]")]
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// The major version number based on <see cref="ApiVersionInheritedAttribute"/>
        /// </summary>
        protected int VersionNumber => HttpContext.GetRequestedApiVersion()?.MajorVersion
            ?? throw new InvalidOperationException("Missing ApiVersionInherited decoration on base version controller");

        internal protected bool VersionedHealthCheck(HealthCheckRegistration registration)
        {
            //no tags present
            if (registration.Tags.Count == 0)
            {
                return true;
            }

            //no tags that start with 'v'
            if (!registration.Tags.Any(x => x.StartsWith('v')))
            {
                return true;
            }

            //has tag that matches this controller's ApiVersion
            if (registration.Tags.Any(x => x == $"v{VersionNumber}"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns status code of the result's StatusType
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        internal protected ActionResult Result<T>(Result<T> result)
        {
            return result.IsSuccess 
                ? Ok(result.Value) 
                : StatusCode((int)result.StatusType, result.ToProblemDetails(HttpContext));
        }

        /// <summary>
        /// Returns status code of the result's StatusType
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        internal protected ActionResult Result(Result result)
        {
            return result.IsSuccess
                ? Ok(result.Value)
                : StatusCode((int)result.StatusType, result.ToProblemDetails(HttpContext));
        }

        /// <summary>
        /// Allows custom success responses like a 201
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="successFunc"></param>
        /// <returns></returns>
        internal protected ActionResult Result<T>(Result<T> result, Func<ActionResult> successFunc)
        {
            return result.IsSuccess
                ? successFunc()
                : StatusCode((int)result.StatusType, result.ToProblemDetails(HttpContext));
        }
    }
}
