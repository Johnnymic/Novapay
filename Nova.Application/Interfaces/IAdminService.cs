using Nova.Application.Dto.Request;
using Nova.Application.Dto.Response;
using Nova.Application.Dto.Response.Nova.Application.Dto.Response;

namespace Nova.Application.Interfaces
{
    public interface IAdminService
    {
      

        Task<ResponseModel< WalletResponse>> FreezeWalletAsync( Guid walletId, FreezeWalletRequest request,string actor,string traceId);

        Task<ResponseModel<WalletResponse>> UnfreezeWalletAsync( Guid walletId, UnfreezeWalletRequest request, string actor, string traceId);

        Task <ResponseModel<List<AdminCustomerWalletResponse>>> GetAllCustomersWithWalletsAsync(string traceId);

    }
}