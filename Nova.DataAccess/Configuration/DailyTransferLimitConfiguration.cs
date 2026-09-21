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
    public class DailyTransferLimitConfiguration
      : IEntityTypeConfiguration<DailyTransferLimit>
    {
        public void Configure(EntityTypeBuilder<DailyTransferLimit> builder)
        {
            builder.ToTable("DailyTransferLimits");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.WalletId)
                .IsRequired();

            builder.Property(x => x.BusinessDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.LimitAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.UsedAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasDefaultValue(0m);

            builder.HasOne(x => x.Wallet)
                .WithMany()
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            // One daily limit record per wallet per business day
            builder.HasIndex(x => new
            {
                x.WalletId,
                x.BusinessDate
            })
            .IsUnique();
        }
    }
}
