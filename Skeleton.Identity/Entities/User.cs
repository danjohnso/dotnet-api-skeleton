using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skeleton.EntityFrameworkCore;
using Skeleton.EntityFrameworkCore.PostgreSQL;

namespace Skeleton.Identity.Entities
{
    public class User : IdentityUser<Guid>
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? Deactivated { get; set; }
        public string? DeactivatedReason { get; set; }
        public DateTime? LastPasswordChange { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime Created { get; private set; }
        public Guid CreatedById { get; set; }
        public DateTime Modified { get; set; }
        public Guid ModifiedById { get; set; }
        public uint RowVersion { get; set; }

        #region - Navigation Properties -		
        public ICollection<AuditEvent> AuditEvents { get; } = [];
        public ICollection<IdentityUserClaim<Guid>> Claims { get; } = [];
        public ICollection<IdentityUserLogin<Guid>> Logins { get; } = [];
        public ICollection<IdentityUserToken<Guid>> Tokens { get; set; } = [];
        public ICollection<PreviousPassword> PreviousPasswords { get; } = [];
        #endregion
    }

    internal static class UserMapping
    {
        public static EntityTypeBuilder<User> Map(this EntityTypeBuilder<User> entityBuilder)
        {
            entityBuilder.ToTable(nameof(User));
            entityBuilder.HasKey(x => x.Id);
            entityBuilder.Property(x => x.Id).MapPrimaryKey();
            entityBuilder.Property(x => x.RowVersion).IsRowVersion();
            entityBuilder.Property(x => x.Created).MapTimestamp();
            entityBuilder.Property(x => x.Modified).MapTimestamp();
            entityBuilder.Property(x => x.FirstName).IsRequired().IsFirstName();
            entityBuilder.Property(x => x.LastName).IsRequired().IsLastName();
            entityBuilder.Property(x => x.DeactivatedReason).HasMaxLength(1000);

            entityBuilder.HasMany(e => e.Tokens)
                         .WithOne()
                         .HasForeignKey(ut => ut.UserId)
                         .IsRequired()
                         .OnDelete(DeleteBehavior.Cascade);

            entityBuilder.HasMany(e => e.Logins)
                         .WithOne()
                         .HasForeignKey(e => e.UserId)
                         .IsRequired()
                         .OnDelete(DeleteBehavior.Cascade);

            entityBuilder.HasMany(e => e.Claims)
                         .WithOne()
                         .HasForeignKey(e => e.UserId)
                         .IsRequired()
                         .OnDelete(DeleteBehavior.Cascade);

            entityBuilder.HasMany(e => e.AuditEvents)
                         .WithOne(x => x.User)
                         .HasForeignKey(e => e.UserId)
                         .IsRequired()
                         .OnDelete(DeleteBehavior.Cascade);

            entityBuilder.HasMany(e => e.PreviousPasswords)
                         .WithOne(x => x.User)
                         .HasForeignKey(e => e.UserId)
                         .IsRequired()
                         .OnDelete(DeleteBehavior.Cascade);

            return entityBuilder;
        }
    }
}
