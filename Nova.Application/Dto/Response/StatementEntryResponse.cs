using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Dto.Response
{
    public class StatementEntryResponse
    {
        public Guid TransactionId { get; set; }
        public Guid? TransferId { get; set; }
        public string Type { get; set; } = default!; // TransactionType.ToString()
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Reference { get; set; } = default!;
        public DateTime CreatedAtUtc { get; set; }
    }

    public class WalletStatementResponse
    {
        public Guid WalletId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public long TotalCount { get; set; }
        public List<StatementEntryResponse> Items { get; set; } = new();
    }
}
