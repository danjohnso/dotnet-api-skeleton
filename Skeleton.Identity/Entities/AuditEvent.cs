using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skeleton.Identity.Enums;

namespace Skeleton.Identity.Entities
{
    public class AuditEvent
    {
        public Guid Id { get; set; }

        public Guid UserId { get; private set; }

        public AuditEventType EventType { get; private set; }

        public DateTime Timestamp { get; private set; }

        public string Message { get; private set; }

        public string TriggeredBy { get; private set; }

        public string IPAddress { get; private set; }

        public User User { get; set; }

        public static AuditEvent CreateAuditEvent(Guid userId, AuditEventType eventType, string ipAddress, string triggeredBy = "Anonymous", string message = null)
        {
            return new AuditEvent
            {
                UserId = userId,
                Message = message,
                EventType = eventType,
	            TriggeredBy = triggeredBy,
                IPAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    internal static class AuditEventMapping
    {
        public static EntityTypeBuilder<AuditEvent> Map(this EntityTypeBuilder<AuditEvent> entityBuilder)
        {
			entityBuilder.ToTable(nameof(AuditEvent));
            entityBuilder.HasKey(x => x.Id);
	        entityBuilder.Property(x => x.Id).ValueGeneratedOnAdd().HasDefaultValueSql("NEWID()");

            entityBuilder.Property(x => x.TriggeredBy).IsRequired().HasMaxLength(255);
            entityBuilder.Property(x => x.Message).HasMaxLength(1000);
            entityBuilder.Property(x => x.IPAddress).IsRequired().HasMaxLength(45);

            return entityBuilder;
        }
    }
}