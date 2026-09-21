using Nova.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Domain.Entities
{
    public class WalletTransaction
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }

        public Guid? TransferId { get; set; }

        public TransactionType Type { get; set; }

        public decimal Amount { get; set; }

        public decimal AmountBalanceBeforeTransaction { get; set; }

        public decimal AmountBalanceAfterTransaction { get; set; }

        public string Reference { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; }

        public Wallet Wallet { get; set; } = null!;

        public Transfer  Transfer { get; set; }

    }
}
