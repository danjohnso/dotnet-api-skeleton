using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skeleton.Data.Core;
using System.ComponentModel.DataAnnotations.Schema;

namespace Skeleton.Data.Entities
{
    public class User : Entity
    {
        public DateTime Created { get; private set; }
        public required string EmailAddress { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateTime Modified { get; set; }

        [NotMapped]
        public string Name => $"{FirstName} {LastName}";
    }

    public static partial class EntityMappings
    {
        public static EntityTypeBuilder<User> Map(this EntityTypeBuilder<User> entity)
        {
            entity.ToTable(nameof(User));
            entity.MapEntity();

            entity.HasIndex(x => x.EmailAddress).IsUnique();

            entity.Property(x => x.Created).ValueGeneratedOnAdd().HasDefaultValueSql("now()");
            entity.Property(x => x.EmailAddress).IsRequired().IsEmailAddress();
            entity.Property(x => x.FirstName).IsRequired().IsFirstName();
            entity.Property(x => x.LastName).IsRequired().IsLastName();
            entity.Property(x => x.Modified).ValueGeneratedOnAdd().HasDefaultValueSql("now()");

            return entity;
        }
    }
}