using Nova.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Domain.Entities
{
    public class IdempotencyRequest
    {
        public Guid Id { get; set; }

        public string Key { get; set; } = null!;

        public string RequestHash { get; set; } = null!;

        public Guid? TransferId { get; set; }

        public int? StatusCode { get; set; }

        public string ResponseBody { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; }
        public Transfer Transfer { get; set; }
        public IdempotencyStatus Status { get; set; }
        public Guid? WalletId { get; set; }
        public string Operation { get; set; }
        public Guid TransactionId { get; set; }
        public string ResourceId { get; set; }
    }
}
