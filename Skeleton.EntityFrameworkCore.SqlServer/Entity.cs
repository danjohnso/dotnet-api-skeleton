using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Skeleton.EntityFrameworkCore.SqlServer
{
    public abstract class Entity
    {
        public Guid Id { get; set; }
        public byte[] RowVersion { get; set; } = null!; // ef core will initialize
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
