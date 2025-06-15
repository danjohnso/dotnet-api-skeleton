using Skeleton.Data.Core;
using System.Collections;

namespace Skeleton.Data.Core
{
    public static class EntityExtensions
    {
        /// <summary>
        /// SQL Server RowVersion
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="rowVersion"></param>
        /// <returns></returns>
        public static bool IsNewerThan(this Entity entity, byte[] rowVersion)
        {
            return StructuralComparisons.StructuralComparer.Compare(entity.RowVersion, rowVersion) > 0;
        }

        /// <summary>
        /// PostgreSQL Row Version
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="rowVersion"></param>
        /// <returns></returns>
        public static bool IsNewerThan(this Entity entity, uint rowVersion)
        {
            return entity.RowVersion != rowVersion;
        }
    }
}
