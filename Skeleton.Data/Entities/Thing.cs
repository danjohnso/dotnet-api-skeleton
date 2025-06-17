using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skeleton.EntityFrameworkCore;
using Skeleton.EntityFrameworkCore.PostgreSQL;

namespace Skeleton.Data.Entities
{
    public class Thing : TrackableEntity
    {
        public required string Name { get; set; }
        public Guid? ParentId { get; set; }

        public List<Thing> Children { get; } = [];
        public Thing? Parent { get; set; }
    }

    public static partial class EntityMappings
    {
        public static EntityTypeBuilder<Thing> Map(this EntityTypeBuilder<Thing> entity)
        {
            entity.ToTable(nameof(Thing));
            entity.MapTrackableEntity();

            entity.Property(x => x.Name).IsRequired().IsName();

            entity.HasOne(x => x.Parent)
               .WithMany(x => x.Children)
               .HasForeignKey(x => x.ParentId)
               .OnDelete(DeleteBehavior.Restrict);

            return entity;
        }
    }
}