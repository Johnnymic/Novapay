using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Dto.Request
{
    public class FreezeWalletRequest
    {
        public string Reason { get; set; } = default!;
    }

    public class UnfreezeWalletRequest
    {
        
        public string? Reason { get; set; }
    }


}
