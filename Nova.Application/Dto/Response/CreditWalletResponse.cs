using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Dto.Response
{
    public class CreditWalletResponse
    {
        public Guid TransactionId { get; set; }

        public Guid WalletId { get; set; }

        public decimal Amount { get; set; }

        public decimal BalanceBefore { get; set; }

        public decimal BalanceAfter { get; set; }

        public string Currency { get; set; } = "NGN";

        public string Reference { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; }
    }
}
