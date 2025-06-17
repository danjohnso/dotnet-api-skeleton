using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections;

namespace Skeleton.EntityFrameworkCore.SqlServer
{
    public static class Extensions
    {
        /// <summary>
        /// Sql Server RowVersion check
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="rowVersion"></param>
        /// <returns></returns>
        public static bool IsNewerThan(this Entity entity, byte[] rowVersion)
        {
            return StructuralComparisons.StructuralComparer.Compare(entity.RowVersion, rowVersion) > 0;
        }

        /// <summary>
        /// Sql Server Timestamp ie Created/Updated in UTC
        /// </summary>
        /// <param name="builder"></param>
        public static void MapTimestamp(this PropertyBuilder<DateTime> builder)
        {
            builder.ValueGeneratedOnAdd().HasDefaultValueSql("GETUTCDATE()");
        }

        /// <summary>
        /// Sql Server GUID Primary Key
        /// </summary>
        /// <param name="builder"></param>
        public static void MapPrimaryKey(this PropertyBuilder<Guid> builder)
        {
            builder.ValueGeneratedOnAdd().HasDefaultValueSql("NEWID()");
        }
    }
}
