using Microsoft.AspNetCore.Mvc;
using Skeleton.API.Core.Core;
using Skeleton.API.Core.ProblemDetails;

namespace Skeleton.API.v1.Controllers
{
    [ApiVersionInherited("1")]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, ProblemDetailsConstants.ContentType)]
    public class BaseVersionController : BaseController
    {
    }

    [ApiVersionInherited("1")]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, ProblemDetailsConstants.ContentType)]
    public class BaseUploadVersionController : BaseUploadController
    {
    }
}
