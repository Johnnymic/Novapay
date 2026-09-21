using Nova.Application.Dto.Request;
using Nova.Application.Dto.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Interfaces
{
    public interface ITransferService
    {

        Task<ResponseModel<CreditWalletResponse>> CreditAsync(Guid walletId, CreditWalletRequest request, Guid customerId, string idempotencyKey, string traceId);
     
        Task<ResponseModel<TransferWalletResponse>> TransferAsync(Guid sourceWalletId, TransferWalletRequest request, Guid customerId, string idempotencyKey, string traceId);


    }
}
