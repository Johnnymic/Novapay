using Nova.Application.Dto.Request;
using Nova.Application.Dto.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Interfaces
{
    public interface IWalletService
    {

        Task<ResponseModel<WalletResponse>> GetWalletAsync(Guid customerId);
        Task<ResponseModel<WalletStatementResponse>> GetWalletStatementAsync( Guid walletId, Guid customerId, WalletStatementRequest request);
        Task<ResponseModel<DailyLimitStatusResponse>> GetDailyLimitStatusAsync( Guid walletId, Guid customerId);

    }
}
