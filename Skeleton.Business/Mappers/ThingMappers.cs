using Skeleton.Business.Models;
using Skeleton.Data.Entities;
using Riok.Mapperly.Abstractions;

namespace Skeleton.Business.Mappers
{
    [Mapper]
    internal static partial class ThingMappers
    {
        public static partial IQueryable<ThingModel> SelectModel(this IQueryable<Thing> q);
        public static partial IQueryable<ThingListModel> SelectListModel(this IQueryable<Thing> q);

        [MapperIgnoreSource(nameof(Thing.CreatedById))]
        [MapperIgnoreSource(nameof(Thing.ModifiedById))]
        [MapperIgnoreSource(nameof(Thing.Parent))]
        private static partial ThingModel Map(Thing entity);

        [MapperIgnoreSource(nameof(Thing.CreatedById))]
        [MapperIgnoreSource(nameof(Thing.ModifiedById))]
        [MapperIgnoreSource(nameof(Thing.Parent))]
        [MapperIgnoreSource(nameof(Thing.RowVersion))]
        [MapperIgnoreSource(nameof(Thing.Created))]
        [MapperIgnoreSource(nameof(Thing.Modified))]
        [MapperIgnoreSource(nameof(Thing.Children))]
        [MapperIgnoreSource(nameof(Thing.IsDeleted))]
        [MapperIgnoreSource(nameof(Thing.ParentId))]
        private static partial ThingListModel MapList(Thing entity);
    }
}
