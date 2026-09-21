using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Dto.Response
{
    public class DailyLimitStatusResponse
    {
        public Guid WalletId { get; set; }
        public DateOnly BusinessDate { get; set; }
        public decimal LimitAmount { get; set; }
        public decimal UsedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}
