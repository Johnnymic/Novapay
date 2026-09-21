using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Dto.Request
{
    public class WalletStatementRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Type { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
