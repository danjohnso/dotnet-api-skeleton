using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Skeleton.EntityFrameworkCore.PostgreSQL
{
    public static class EntityExtensions
    {
        /// <summary>
        /// PostgreSQL Row Version check
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="rowVersion"></param>
        /// <returns></returns>
        public static bool IsNewerThan(this Entity entity, uint rowVersion)
        {
            return entity.RowVersion != rowVersion;
        }

        /// <summary>
        /// PostgreSQL Timestamp ie Created/Updated in UTC
        /// </summary>
        /// <param name="builder"></param>
        public static void MapTimestamp(this PropertyBuilder<DateTime> builder)
        {
            builder.ValueGeneratedOnAdd().HasDefaultValueSql("now()");
        }

        /// <summary>
        /// PostgreSQL GUID Primary Key
        /// </summary>
        /// <param name="builder"></param>
        public static void MapPrimaryKey(this PropertyBuilder<Guid> builder)
        {
            builder.ValueGeneratedOnAdd().HasDefaultValueSql("gen_random_uuid()");
        }
    }
}
