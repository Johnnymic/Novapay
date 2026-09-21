using Microsoft.EntityFrameworkCore;
using Nova.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.DataAccess.Persistence
{
    public class NovaWalletDbContext : DbContext
    {
        public NovaWalletDbContext(
            DbContextOptions<NovaWalletDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Wallet> Wallets => Set<Wallet>();
        public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
        public DbSet<Transfer> Transfers => Set<Transfer>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<DailyTransferLimit> DailyTransferLimits => Set<DailyTransferLimit>();
        public DbSet<IdempotencyRequest> IdempotencyRequests => Set<IdempotencyRequest>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(NovaWalletDbContext).Assembly);
        }
    }
}
