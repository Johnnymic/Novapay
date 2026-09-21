using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Dto.Response
{
    namespace Nova.Application.Dto.Response
    {
        public class ReverseTransferResponse
        {
            public Guid OriginalTransferId { get; set; }
            public Guid ReversalTransferId { get; set; }

            public Guid SourceWalletId { get; set; }

            public Guid DestinationWalletId { get; set; }

            public decimal Amount { get; set; }
            public string Currency { get; set; } = default!;
            public string Reason { get; set; } = default!;
            public string Status { get; set; } = default!;
            public DateTime CreatedAtUtc { get; set; }
            public string TraceId { get; set; } = default!;
        }
    }
}
