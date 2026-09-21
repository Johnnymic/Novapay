using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Dto.Request
{
    public class CreditWalletRequest
    {
        public decimal Amount { get; set; }

        public string Reference { get; set; } = null!;

        public string? Description { get; set; }
    }
}
