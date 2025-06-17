using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Skeleton.EntityFrameworkCore.SqlServer
{
    public abstract class TrackableEntity : Entity
    {
        public DateTime Created { get; private set; }
        public Guid CreatedById { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime Modified { get; set; }
        public Guid ModifiedById { get; set; }
    }

    public static partial class EntityMappings
    {
        public static EntityTypeBuilder<T> MapTrackableEntity<T>(this EntityTypeBuilder<T> entity) where T : TrackableEntity
        {
            entity.MapEntity();

            entity.Property(x => x.Created).MapTimestamp();
            entity.Property(x => x.Modified).MapTimestamp();

            entity.Property(x => x.IsDeleted).HasDefaultValue(false);

            return entity;
        }
    }
}
