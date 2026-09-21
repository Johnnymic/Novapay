using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Domain.Entities
{
    public class DailyTransferLimit
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }

        public DateOnly BusinessDate { get; set; }

        public decimal LimitAmount { get; set; }

        public decimal UsedAmount { get; set; }

        public Wallet Wallet { get; set; } = null!;
    }

}
