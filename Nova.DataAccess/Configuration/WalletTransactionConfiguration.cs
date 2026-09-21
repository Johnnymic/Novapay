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
    public class WalletTransactionConfiguration
    : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.ToTable("WalletTransactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.WalletId)
                .IsRequired();

            builder.Property(x => x.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.AmountBalanceBeforeTransaction)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.AmountBalanceAfterTransaction)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.Reference)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.HasOne(x => x.Wallet)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            // Transaction references must be unique
            builder.HasIndex(x => x.Reference);

            // Useful for wallet statements
            builder.HasIndex(x => new
            {
                x.WalletId,
                x.CreatedAtUtc
            });
        }
    }
}
