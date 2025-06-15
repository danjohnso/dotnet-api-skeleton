using Skeleton.Business.Mappers;
using Skeleton.Data;
using Microsoft.EntityFrameworkCore;
using Skeleton.Business.Models;

namespace Skeleton.Business.Services
{
    public class UserService(DataContext _dataContext)
    {
        public Task<ProfileModel?> GetAsync(Guid id)
        {
            return _dataContext.Users.Where(x => x.Id == id).SelectProfileModel().FirstOrDefaultAsync();
        }

        public Task<string> GetDisplayNameAsync(Guid id)
        {
            return _dataContext.Users.Where(x => x.Id == id).Select(x => x.Name).SingleAsync();
        }
    }
}
