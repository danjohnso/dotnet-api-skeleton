using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Skeleton.Data.Core
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

            entity.Property(x => x.Created).ValueGeneratedOnAdd().HasDefaultValueSql("now()");
            entity.Property(x => x.Modified).ValueGeneratedOnAdd().HasDefaultValueSql("now()");

            entity.Property(x => x.IsDeleted).HasDefaultValue(false);

            return entity;
        }
    }
}
