using Skeleton.Business.Mappers;
using Skeleton.Business.Models;
using Skeleton.Core;
using Skeleton.Data;
using Skeleton.Data.Core;
using Skeleton.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Skeleton.Business.Services
{
    public class ThingService(ILogger<ThingService> _logger, DataContext _dataContext)
    {
        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="modelNumber"></param>
        /// <param name="parentId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Result<Guid>> CreateAsync(string name, Guid? parentId, Guid userId)
        {
            _logger.LogInformation("Starting CreateAsync with {Name}, {ParentId}, {UserId}",
                 name, parentId, userId);

            if (parentId.HasValue)
            {
                _logger.LogWarning("Unable to find thing {ThingId} for user {UserId}", parentId.Value, userId);
                return Result<Guid>.NotFound(Problems.NotFound);
            }

            Thing entity = new()
            {
                CreatedById = userId,
                ModifiedById = userId,
                Name = name,
                ParentId = parentId
            };

            _dataContext.Things.Add(entity);

            await _dataContext.SaveChangesAsync();

            _logger.LogDebug("Completing CreateAsync");

            return Result.Success(entity.Id);
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Result> DeleteAsync(Guid id, Guid userId)
        {
            _logger.LogInformation("Starting DeleteAsync with {ThingId}, {UserId}", id, userId);

            Thing? entity = await _dataContext.Things.SingleOrDefaultAsync(x => x.Id == id);
            if (entity is null)
            {
                _logger.LogWarning("Unable to find thing {ThingId} in user {UserId}", id, userId);
                return Result.NotFound(Problems.NotFound);
            }

            if (!entity.IsDeleted)
            {
                entity.IsDeleted = true;
                entity.Modified = DateTime.UtcNow;
                entity.ModifiedById = userId;

                await _dataContext.SaveChangesAsync();
            }

            _logger.LogDebug("Completing DeleteAsync");

            return Result.Success();
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Result<ThingModel>> GetDetailAsync(Guid id, Guid userId)
        {
            _logger.LogInformation("Starting GetDetailAsync with {ThingId}, {UserId}", id, userId);

            ThingModel? response = await _dataContext.Things.Where(x => x.Id == id).SelectModel().SingleOrDefaultAsync();
            if (response is null)
            {
                _logger.LogWarning("Unable to find thing {ThingId} in user {UserId}", id, userId);
                return Result<ThingModel>.NotFound(Problems.NotFound);
            }

            _logger.LogDebug("Completing GetDetailAsync with {@Response}", response);

            return Result.Success(response);
        }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isDeleted"></param>
        /// <returns></returns>
        public async Task<Result<List<ThingListModel>>> GetListAsync(Guid userId, bool isDeleted = false)
        {
            _logger.LogInformation("Starting GetListAsync for User {UserId}", userId);

            List<ThingListModel> response = await _dataContext.Things.Where(x => x.IsDeleted == isDeleted).SelectListModel().ToListAsync();

            _logger.LogDebug("Completing GetListAsync with {@Response}", response);

            return Result.Success(response);
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Result> RestoreAsync(Guid id, Guid userId)
        {
            _logger.LogInformation("Starting RestoreAsync with {ThingId}, {UserId}", id, userId);

            Thing? entity = await _dataContext.Things.SingleOrDefaultAsync(x => x.Id == id);
            if (entity is null)
            {
                _logger.LogWarning("Unable to find thing {ThingId} in user {UserId}", id, userId);
                return Result.NotFound(Problems.NotFound);
            }

            if (entity.IsDeleted)
            {
                entity.IsDeleted = false;
                entity.Modified = DateTime.UtcNow;
                entity.ModifiedById = userId;

                await _dataContext.SaveChangesAsync();
            }

            _logger.LogDebug("Completing RestoreAsync");

            return Result.Success();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="parentId"></param>
        /// <param name="rowVersion"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Result> UpdateAsync(Guid id, string name, Guid? parentId, uint rowVersion, Guid userId)
        {
            _logger.LogInformation("Starting UpdateAsync with {Id}, {Name}, {ParentId}, {RowVersion}, {UserId}",
                id, name, parentId, rowVersion, userId);

            Thing? entity = await _dataContext.Things.SingleOrDefaultAsync(x => x.Id == id);
            if (entity is null)
            {
                _logger.LogWarning("Unable to find thing {ThingId} in user {UserId}", id, userId);
                return Result.NotFound(Problems.NotFound);
            }

            if (entity.IsNewerThan(rowVersion))
            {
                _logger.LogWarning("Concurrency conflict for thing {ThingId}", id);
                return Result.Conflict(Problems.Conflict);
            }

            if (entity.IsDeleted)
            {
                _logger.LogWarning("TAMPER WARN: Deleted things can't be updated {ThingId}", id);
                return Result.Invalid(Problems.Deleted);
            }

            entity.Name = name;            
            entity.ParentId = parentId;
            entity.Modified = DateTime.UtcNow;
            entity.ModifiedById = userId;

            await _dataContext.SaveChangesAsync();

            _logger.LogDebug("Completing UpdateAsync");

            return Result.Success();
        }
    }
}
