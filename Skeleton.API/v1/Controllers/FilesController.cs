using Skeleton.API.Streaming;
using Skeleton.Business.Models;
using Skeleton.Business.Services;
using Skeleton.Core;
using Skeleton.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Skeleton.API.Core.Auth;
using Skeleton.API.Core.ProblemDetails;
using Skeleton.API.Core.Validation.File;
using Skeleton.API.v1.Requests;
using System.Net.Mime;

namespace Skeleton.API.v1.Controllers
{
    public class FilesController(ILogger<FilesController> _logger, FileService _fileService) : BaseUploadVersionController
    {
        private const long MaxFileSizeInMB = 10;
        private const long MaxFileSizeInBytes = MaxFileSizeInMB * 1024 * 1024;

        [HttpDelete("{id:guid}/{parentId:guid}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, ProblemDetailsConstants.ContentType)]
        public async Task<IActionResult> Delete(Guid id, Guid parentId)
        {
            if (id == Guid.Empty)
            {
                return Result(Skeleton.Core.Result.NotFound(Problems.NotFound));
            }

            Result result = await _fileService.DeleteAsync(id, parentId, User.GetId());
            return Result(result);
        }

        [HttpGet("list/{parentId:guid}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(IEnumerable<FileListModel>), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, ProblemDetailsConstants.ContentType)]
        public async Task<IActionResult> List(Guid parentId)
        {
            if (parentId == Guid.Empty)
            {
                return Result(Skeleton.Core.Result.NotFound(Problems.NotFound));
            }

            Result<List<FileListModel>> result = await _fileService.GetListAsync(parentId, User.GetId());
            return Result(result);
        }

        [HttpPut("upload")]
        [RequestSizeLimit(MaxFileSizeInBytes)]
        [Consumes(MediaTypeNames.Multipart.FormData)]
        [DisableFormValueModelBinding]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, ProblemDetailsConstants.ContentType)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, ProblemDetailsConstants.ContentType)]
        public async Task<IActionResult> Upload([FromForm]FileUploadRequest request)
        {
            if (request.ParentId == Guid.Empty)
            {
                return Result(Skeleton.Core.Result.NotFound(Problems.NotFound));
            }

            if (!FileValidator.Exists(request.File))
            {
                return Result(Skeleton.Core.Result.Invalid(Problems.FileEmpty));
            }

            if (!FileValidator.IsValidSize(request.File, MaxFileSizeInBytes))
            {
                return Result(Skeleton.Core.Result.Invalid(Problems.FileEmpty));
            }

            FileFormatValidationResult formatResult = FileValidator.IsValidFormat(request.File);
            if (!formatResult.IsAcceptable)
            {
                _logger.LogCritical("Invalid file type attempted to be uploaded by {UserId}.  Result: {@Result}", User.GetId(), formatResult);
                return Result(Skeleton.Core.Result.Invalid(Problems.FileTypeInvalid));
            }

#warning do we need to reset the stream position?  FormatValidator also calls OpenReadStream
            //stream.Position = 0;

            Result<string> result = await _fileService.UploadAsync(request.ParentId, request.File.FileName, request.File.OpenReadStream(), User.GetId());
            return Result(result, () => Created(result.Value, null));
        }
    }
}
