﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skeleton.EntityFrameworkCore;
using Skeleton.EntityFrameworkCore.PostgreSQL;
using System.ComponentModel.DataAnnotations.Schema;

namespace Skeleton.Data.Entities
{
    /// <summary>
    /// This is one option if you don't use identity to maintain a user table, see <see cref="UserProfile"/> for a way top linke when using Identity
    /// </summary>
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

            entity.Property(x => x.Created).MapTimestamp();
            entity.Property(x => x.EmailAddress).IsRequired().IsEmailAddress();
            entity.Property(x => x.FirstName).IsRequired().IsFirstName();
            entity.Property(x => x.LastName).IsRequired().IsLastName();
            entity.Property(x => x.Modified).MapTimestamp();

            return entity;
        }
    }
}