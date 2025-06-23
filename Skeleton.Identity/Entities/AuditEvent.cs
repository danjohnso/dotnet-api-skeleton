using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skeleton.EntityFrameworkCore;
using Skeleton.EntityFrameworkCore.PostgreSQL;
using Skeleton.Identity.Enums;

namespace Skeleton.Identity.Entities
{
    public class AuditEvent
    {
        public Guid Id { get; private set; }
        public AuditEventType EventType { get; set; }
        public required string IPAddress { get; set; }
        public string? Message { get; set; }
        public DateTime TimeStamp { get; private set; }
        /// <summary>
        /// User or system that triggered the event
        /// </summary>
        public required string TriggeredBy { get; set; }
        public Guid UserId { get; set; }

        public User User { get; set; } = null!; // ef core will initialize if the queries are correct, this is a required relationship
    }

    internal static class AuditEventMapping
    {
        public static EntityTypeBuilder<AuditEvent> Map(this EntityTypeBuilder<AuditEvent> entityBuilder)
        {
			entityBuilder.ToTable(nameof(AuditEvent));
            entityBuilder.HasKey(x => x.Id);

            entityBuilder.Property(x => x.TimeStamp).MapTimestamp();
            entityBuilder.Property(x => x.Id).MapPrimaryKey();
            entityBuilder.Property(x => x.TriggeredBy).IsRequired().HasMaxLength(255);
            entityBuilder.Property(x => x.Message).HasMaxLength(1000);
            entityBuilder.Property(x => x.IPAddress).IsRequired().IsIPAddress();

            return entityBuilder;
        }
    }
}