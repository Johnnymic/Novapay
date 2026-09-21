using Nova.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Domain.Entities
{
    public class Transfer
    {
        

        public Guid Id { get; set; }

        public Guid SourceWalletId { get; set; }

        public Guid ReversalOfTransferId { get;set; }

        public Guid DestinationWalletId { get; set; }

        public decimal Amount { get; set; }

        public string Reference { get; set; } = null!;

        public TransferStatus Status { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? CompletedAtUtc { get; set; }

      

      public Wallet SourceWallet { get; set; } = null!;

        public Wallet DestinationWallet { get; set; } = null!;

        public ICollection<WalletTransaction> Transactions { get; set; }
            = new List<WalletTransaction>();
      
    }
}
