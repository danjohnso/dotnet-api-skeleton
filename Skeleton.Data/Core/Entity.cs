using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Skeleton.Data.Core
{
    public abstract class Entity
    {
        public Guid Id { get; set; }
        public uint RowVersion { get; set; }
    }

    public static partial class EntityMappings
    {
        public static EntityTypeBuilder<T> MapEntity<T>(this EntityTypeBuilder<T> entity) where T : Entity
        {
            entity.HasKey(x => x.Id);

            //https://www.npgsql.org/efcore/modeling/generated-properties.html?tabs=13%2Cefcore5#guiduuid-generation
            entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()"); 

            entity.Property(x => x.RowVersion).IsRowVersion();

            return entity;
        }
    }
}
