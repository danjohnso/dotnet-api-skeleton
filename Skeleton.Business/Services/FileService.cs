using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Skeleton.Business.Mappers;
using Skeleton.Business.Models;
using Skeleton.Business.Repositories;
using Skeleton.Core;
using Skeleton.Data;
using Skeleton.Data.Entities;

namespace Skeleton.Business.Services
{
#warning need to security checks with whatever your associations are
    public class FileService(ILogger<FileService> _logger, IMemoryCache _memoryCache, AzureStorageRepository _azureStorageRepository, DataContext _dataContext)
    {
        public async Task<Result> DeleteAsync(Guid id, Guid parentId, Guid userId)
        {
            AppFile? file = await _dataContext.AppFiles.SingleOrDefaultAsync(x => x.Id == id);
            if (file is null)
            {
                _logger.LogWarning("Unable to find file {FileId}", id);
                return Result.NotFound(Problems.NotFound);
            }

            bool isDeleted = await _azureStorageRepository.DeleteAsync(parentId.ToString(), file.FileName);
            if (isDeleted)
            {
                _dataContext.AppFiles.Remove(file);

                await _dataContext.SaveChangesAsync();

                return Result.Success();
            }

            return Result.Error(Problems.UnexpectedError);
        }

        public async Task<Result<List<FileListModel>>> GetListAsync(Guid parentId, Guid userId)
        {
            List<FileListModel>? files = await _memoryCache.GetOrCreateAsync($"files-{parentId}", async cacheEntry =>
            {
                List<FileListModel> files = await _dataContext.AppFiles
                    //.Where(x => x.ParentId == parentId)
                    .SelectListModel()
                    .ToListAsync();

                if (files.Count == 0)
                {
                    cacheEntry.Dispose();
                    return null;
                }

                Dictionary<string, string> downloadLinks = await _azureStorageRepository.GetLinksAsync(parentId.ToString(), files.Select(x => x.FileName));
                foreach (FileListModel file in files)
                {
                    file.Link = downloadLinks[file.FileName];
                }

                //should cache less than the token lifetime
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_azureStorageRepository.SASLifetimeMinutes - 1);

                return files;
            });

            return Result.Success(files ?? []);
        }

        public async Task<Result<string>> UploadAsync(Guid parentId, string fileName, Stream file, Guid userId)
        {
            string? downloadUrl = await _azureStorageRepository.UploadAsync(parentId.ToString(), fileName, file);
            if (downloadUrl != null)
            {
                AppFile entity = new()
                {
                    FileName = fileName,
                    //ParentId = parentId
                };

                _dataContext.AppFiles.Add(entity);

                await _dataContext.SaveChangesAsync();

                await _azureStorageRepository.UpdateMetadataAsync(parentId.ToString(), fileName, new Dictionary<string, string>
                {
                    { "app_file_id", entity.Id.ToString() }
                });

                //invalidate the file list cache on upload
                _memoryCache.Remove($"files-{parentId}");

                return Result.Success(downloadUrl);
            }

            return Result<string>.Error(Problems.UnexpectedError);
        }
    }
}
