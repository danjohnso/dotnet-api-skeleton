using Skeleton.Business.Models;
using Skeleton.Data.Entities;
using Riok.Mapperly.Abstractions;

namespace Skeleton.Business.Mappers
{
    [Mapper]
    internal static partial class FileMappers
    {
        public static partial IQueryable<FileListModel> SelectListModel(this IQueryable<AppFile> q);

        [MapperIgnoreTarget(nameof(FileListModel.Link))] //this is set in service since we have to iterate storage
        //[MapperIgnoreSource(nameof(AppFile.ParentId))]
        [MapperIgnoreSource(nameof(AppFile.RowVersion))]
        private static partial FileListModel MapList(AppFile entity);
    }
}
