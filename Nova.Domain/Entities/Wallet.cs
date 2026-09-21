using Nova.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Domain.Entities
{
    public class Wallet
    {

        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }

        public string Currency { get; set; }

        public decimal Balance { get; set; }

        public WalletStatus Status { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public Customer Customer { get; set; } = null!;

        public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
       
    }
}
