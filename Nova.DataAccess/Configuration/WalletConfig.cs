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
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("Wallets");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.CustomerId)
                .IsRequired();

            builder.Property(x => x.Currency)
                .HasMaxLength(3)
                .IsRequired()
                .HasDefaultValue("NGN");

            builder.Property(x => x.Balance)
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasDefaultValue(0m);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            // Customer → Wallet
            builder.HasOne(x => x.Customer)
                .WithMany(x => x.Wallets)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Wallet → Transactions
            builder.HasMany(x => x.Transactions)
                .WithOne(x => x.Wallet)
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            // One customer should not have multiple NGN wallets
            builder.HasIndex(x => new { x.CustomerId, x.Currency })
                .IsUnique();
        }
    }
    } 

