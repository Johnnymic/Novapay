using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Dto.Response
{
    public class TransferWalletResponse
    {
        public Guid TransferId { get; set; }

        public Guid SourceWalletId { get; set; }

        public Guid DestinationWalletId { get; set; }

        public decimal Amount { get; set; }

        public decimal SourceBalanceBefore { get; set; }

        public decimal SourceBalanceAfter { get; set; }

        public decimal DestinationBalanceBefore { get; set; }

        public decimal DestinationBalanceAfter { get; set; }

        public string Currency { get; set; } = "NGN";

        public string Reference { get; set; } = null!;

        public string Status { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; }

        public string TraceId { get; set; } = null!;
    }
}
