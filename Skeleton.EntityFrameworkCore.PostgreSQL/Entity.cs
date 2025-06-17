using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Skeleton.EntityFrameworkCore.PostgreSQL
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

            entity.Property(x => x.Id).MapPrimaryKey();
            entity.Property(x => x.RowVersion).IsRowVersion();

            return entity;
        }
    }
}
