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
    public class TransferConfiguration
    : IEntityTypeConfiguration<Transfer>
    {
        public void Configure(EntityTypeBuilder<Transfer> builder)
        {
            builder.ToTable("Transfers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.SourceWalletId)
                .IsRequired();

            builder.Property(x => x.ReversalOfTransferId);

            builder.Property(x => x.DestinationWalletId)
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.Reference)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.CompletedAtUtc)
                .IsRequired(false);

            // Source wallet
            builder.HasOne(x => x.SourceWallet)
                .WithMany()
                .HasForeignKey(x => x.SourceWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            // Destination wallet
            builder.HasOne(x => x.DestinationWallet)
                .WithMany()
                .HasForeignKey(x => x.DestinationWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Reference)
                .IsUnique();

            builder.HasIndex(x => x.SourceWalletId);

            builder.HasIndex(x => x.DestinationWalletId);
        }
    }
}