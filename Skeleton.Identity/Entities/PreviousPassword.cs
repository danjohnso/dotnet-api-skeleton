using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skeleton.EntityFrameworkCore.PostgreSQL;

namespace Skeleton.Identity.Entities
{
    public class PreviousPassword(string hash, Guid userId)
    {
        public Guid Id { get; private set; }
        public required string PasswordHash { get; set; } = hash;
        public Guid UserId { get; private set; } = userId;
        public DateTime Created { get; private set; }
        public User User { get; set; }
    }

    internal static class PreviousPassworSkeletonpping
    {
        public static EntityTypeBuilder<PreviousPassword> Map(this EntityTypeBuilder<PreviousPassword> entity)
        {
			entity.ToTable(nameof(PreviousPassword));
	        entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).MapPrimaryKey();
            entity.Property(x => x.Created).MapTimestamp();

            return entity;
        }
    }
}