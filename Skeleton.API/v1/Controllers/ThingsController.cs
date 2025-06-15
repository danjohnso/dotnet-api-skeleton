using Skeleton.API.Core.Auth;
using Skeleton.API.Core.ProblemDetails;
using Skeleton.API.Core.Validation;
using Skeleton.API.v1.Requests;
using Skeleton.Business.Models;
using Skeleton.Business.Services;
using Skeleton.Core;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Skeleton.API.v1.Controllers
{
    public class ThingsController(ThingService _thingService) : BaseVersionController
    {
        private const string ControllerName = "things";

        [HttpGet("")]
        [ProducesResponseType(typeof(IEnumerable<ThingListModel>), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, ProblemDetailsConstants.ContentType)]
        public async Task<IActionResult> List()
        {
            Result<List<ThingListModel>> result = await _thingService.GetListAsync(User.GetId());
            return Result(result);
        }

        [HttpGet("deleted")]
        [ProducesResponseType(typeof(IEnumerable<ThingListModel>), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, ProblemDetailsConstants.ContentType)]
        public async Task<IActionResult> Deleted()
        {
            Result<List<ThingListModel>> result = await _thingService.GetListAsync(User.GetId(), isDeleted: true);
            return Result(result);
        }

        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, ProblemDetailsConstants.ContentType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, ProblemDetailsConstants.ContentType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity, ProblemDetailsConstants.ContentType)]
        [ValidationFilter<ThingCreateRequest>]
        public async Task<IActionResult> Create(ThingCreateRequest request)
        {
            Result<Guid> result = await _thingService.CreateAsync(request.Name, request.ParentId, User.GetId());
            return Result(result, () => CreatedAtAction(nameof(Get), controllerName: ControllerName, routeValues: new { id = result.Value, version = $"{VersionNumber}" }, value: null));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ThingModel), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, ProblemDetailsConstants.ContentType)]
        public async Task<IActionResult> Get(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Result(Skeleton.Core.Result.NotFound(Problems.NotFound));
            }

            Result<ThingModel> result = await _thingService.GetDetailAsync(id, User.GetId());
            return Result(result);
        }

        [HttpPatch("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, ProblemDetailsConstants.ContentType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, ProblemDetailsConstants.ContentType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict, ProblemDetailsConstants.ContentType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity, ProblemDetailsConstants.ContentType)]
        [ValidationFilter<ThingUpdateRequest>]
        public async Task<IActionResult> Update(Guid id, ThingUpdateRequest request)
        {
            if (id == Guid.Empty)
            {
                return Result(Skeleton.Core.Result.NotFound(Problems.NotFound));
            }

            //validator will ensure we have a value in row version
            Result result = await _thingService.UpdateAsync(id, request.Name, request.ParentId, request.RowVersion!.Value, User.GetId());
            return Result(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, ProblemDetailsConstants.ContentType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, ProblemDetailsConstants.ContentType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict, ProblemDetailsConstants.ContentType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity, ProblemDetailsConstants.ContentType)]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Result(Skeleton.Core.Result.NotFound(Problems.NotFound));
            }

            Result result = await _thingService.DeleteAsync(id, User.GetId());
            return Result(result);
        }

        [HttpPost("{id:guid}/restore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, ProblemDetailsConstants.ContentType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, ProblemDetailsConstants.ContentType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict, ProblemDetailsConstants.ContentType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity, ProblemDetailsConstants.ContentType)]
        public async Task<IActionResult> Restore(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Result(Skeleton.Core.Result.NotFound(Problems.NotFound));
            }

            Result result = await _thingService.RestoreAsync(id, User.GetId());
            return Result(result);
        }
    }
}
