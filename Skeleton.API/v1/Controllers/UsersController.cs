using Skeleton.Business.Models;
using Skeleton.Business.Services;
using Skeleton.Core;
using Microsoft.AspNetCore.Mvc;
using Skeleton.API.Core.Auth;
using Skeleton.API.Core.ProblemDetails;
using System.Net.Mime;

namespace Skeleton.API.v1.Controllers
{
    public class UsersController(UserService _userService) : BaseVersionController
    {
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ProfileModel), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, ProblemDetailsConstants.ContentType)]
        public async Task<IActionResult> Get()
        {
            ProfileModel? model = await _userService.GetAsync(User.GetId());
            if (model == null)
            {
                return Result(Skeleton.Core.Result.NotFound(Problems.NotFound));
            }

            return Ok(model);
        }
    }
}
