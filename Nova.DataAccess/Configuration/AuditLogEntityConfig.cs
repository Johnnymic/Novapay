using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nova.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.DataAccess.Configuration
{
    public class AuditLogConfiguration
    : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.EntityType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.EntityId)
                .IsRequired();

            builder.Property(x => x.Action)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            builder.Property(x => x.Actor)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.TraceId)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(x => x.Metadata)
                    .HasColumnType("json")
                    .IsRequired(false);

            builder.Property(x => x.CreatedAt);

            builder.HasIndex(x => new
            {
                x.EntityType,
                x.EntityId,
                x.CreatedAt
            });
        }
    }
}
