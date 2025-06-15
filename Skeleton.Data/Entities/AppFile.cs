using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skeleton.Data.Core;

namespace Skeleton.Data.Entities
{
    public class AppFile : Entity
    {
        public required string FileName { get; set; }

        //Generally the file is associated to something in the App for associations and security checks
        //public Guid SomeObjectId { get; set; }
        //public SomeObject SomeObject { get; set; } = null!;
    }

    public static partial class EntityMappings
    {
        public static EntityTypeBuilder<AppFile> Map(this EntityTypeBuilder<AppFile> entity)
        {
            entity.ToTable(nameof(AppFile));
            entity.MapEntity();

            entity.Property(x => x.FileName).IsRequired().IsName();

            return entity;
        }
    }
}