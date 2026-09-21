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
    public class IdempotencyRequestConfiguration
         : IEntityTypeConfiguration<IdempotencyRequest>
    {
        public void Configure(
            EntityTypeBuilder<IdempotencyRequest> builder)
        {
            builder.ToTable("IdempotencyRequests");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.Key)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.RequestHash)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(x => x.Operation)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.WalletId)
                .IsRequired(false);

            builder.Property(x => x.ResourceId)
                .IsRequired(false);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.StatusCode)
                .IsRequired(false);

            builder.Property(x => x.ResponseBody)
                .HasColumnType("json")
                .IsRequired(false);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            // Same idempotency key can only exist once.
            builder.HasIndex(x => x.Key)
                .IsUnique();

            // Useful for looking up idempotency records
            // by wallet and operation.
            builder.HasIndex(x => new
            {
                x.WalletId,
                x.Operation
            });

            // Useful when retrieving the resource associated
            // with an idempotency request.
            builder.HasIndex(x => x.ResourceId);
        }
    }

}
