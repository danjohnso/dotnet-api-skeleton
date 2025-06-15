using Skeleton.Data.Entities;
using Riok.Mapperly.Abstractions;
using Skeleton.Business.Models;

namespace Skeleton.Business.Mappers
{
    [Mapper]
    internal static partial class UserMappers
    {
        public static partial IQueryable<ProfileModel> SelectProfileModel(this IQueryable<User> q);

        [MapperIgnoreSource(nameof(User.Name))]
        [MapperIgnoreSource(nameof(User.Modified))]
        [MapperIgnoreSource(nameof(User.Created))]
        [MapperIgnoreSource(nameof(User.RowVersion))]
        private static partial ProfileModel Map(User entity);
    }
}
