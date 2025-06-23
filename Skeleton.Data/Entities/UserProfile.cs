using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skeleton.EntityFrameworkCore;
using Skeleton.EntityFrameworkCore.PostgreSQL;

namespace Skeleton.Data.Entities
{
    /// <summary>
    /// This is a better alternative when using Identity.  Create a one to one relationship with the Identity User table and use this class as the link to your User in navigation properties
    /// </summary>
    public class UserProfile : TrackableEntity
    {
        public string? AvatarUrl { get; set; }
        public required string DisplayName { get; set; }

        public Guid UserId { get; set; }
        //Note this should reference the Identity.Entities.User class, there won't be a User class in this project
        public User User { get; set; } = null!;
    }

    public static partial class EntityMappings
    {
        public static EntityTypeBuilder<UserProfile> Map(this EntityTypeBuilder<UserProfile> entity)
        {
            entity.ToTable(nameof(UserProfile));
            entity.MapTrackableEntity();

            entity.Property(x => x.AvatarUrl).IsUrl();
            entity.Property(x => x.DisplayName).IsRequired().IsName();

            //one to one to link to the user
            entity.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<UserProfile>(x => x.UserId)
                .IsRequired();

            return entity;
        }
    }
}