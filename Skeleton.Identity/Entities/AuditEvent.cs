using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skeleton.EntityFrameworkCore;
using Skeleton.EntityFrameworkCore.PostgreSQL;
using Skeleton.Identity.Enums;

namespace Skeleton.Identity.Entities
{
    public class AuditEvent(Guid userId, AuditEventType eventType, string ipAddress, string triggeredBy = "Anonymous", string? message = null)
    {
        public Guid Id { get; private set; }
        public AuditEventType EventType { get; private set; } = eventType;
        public string IPAddress { get; private set; } = ipAddress;
        public string? Message { get; private set; } = message;
        public DateTime TimeStamp { get; private set; } = DateTime.UtcNow;
        /// <summary>
        /// User or system that triggered the event
        /// </summary>
        public string TriggeredBy { get; private set; } = triggeredBy;
        public Guid UserId { get; private set; } = userId;
        
        public User User { get; set; } = null!; // ef core will initialize if the queries are correct, this is a required relationship
    }

    internal static class AuditEventMapping
    {
        public static EntityTypeBuilder<AuditEvent> Map(this EntityTypeBuilder<AuditEvent> entityBuilder)
        {
			entityBuilder.ToTable(nameof(AuditEvent));
            entityBuilder.HasKey(x => x.Id);
            entityBuilder.Property(x => x.Id).MapPrimaryKey();

            entityBuilder.Property(x => x.TriggeredBy).IsRequired().HasMaxLength(255);
            entityBuilder.Property(x => x.Message).HasMaxLength(1000);
            entityBuilder.Property(x => x.IPAddress).IsRequired().IsIPAddress();

            return entityBuilder;
        }
    }
}