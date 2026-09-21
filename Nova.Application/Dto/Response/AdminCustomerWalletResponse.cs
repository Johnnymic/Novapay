using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Dto.Response
{
    public class AdminCustomerWalletResponse
    {
        public Guid CustomerId { get; set; }

        public string CustomerReference { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string Email { get; set; } = null!;

        
        public List<AdminWalletResponse> Wallets { get; set; } = new();
    }

    public class AdminWalletResponse
    {
        public Guid WalletId { get; set; }

        public decimal Balance { get; set; }

        public string Currency { get; set; } = null!;

        public string Status { get; set; } = null!;

        public List<AdminTransferResponse> Transfers { get; set; }
            = new List<AdminTransferResponse>();
    }



    public class AdminTransferResponse
    {
        public Guid TransferId { get; set; }

        public Guid SourceWalletId { get; set; }

        public Guid DestinationWalletId { get; set; }

        public decimal Amount { get; set; }

        public string Reference { get; set; } = null!;

        public string Status { get; set; } = null!;

        public string Direction { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? CompletedAtUtc { get; set; }

        public Guid? ReversalOfTransferId { get; set; }
    }

}
