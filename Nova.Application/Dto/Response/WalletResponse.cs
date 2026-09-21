using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Dto.Response
{
    public class WalletResponse
    {
        public Guid walletId { get; set; }

        public Guid CustomerId { get; set; }

        public string Currency { get; set; } = null!;

        public decimal Balance { get; set; }

        public string Status { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
