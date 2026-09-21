using Nova.Application.Dto.Response;
using Nova.Application.Dto.Response.Nova.Application.Dto.Response;
using Nova.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Interfaces
{
    public interface IHelperService 
    {
        Task<Wallet> DoesWalletExist(Guid customerId);

        Task<Wallet> GetWalletAsync(Guid walletId, Guid customerId);

        Task<IdempotencyRequest?> GetIdempotencyRequestAsync(string idempotencyKey);

        Task CheckAndUpdateDailyTransferLimitAsync(Guid walletId,decimal transferAmount);

        Task<CreditWalletResponse>HandleExistingRequestAsync( IdempotencyRequest existingReques, string requestHash);

        Task<Transfer> ValidateTransferForReversal(Guid transferId);

        Task<ReverseTransferResponse> HandleExistingReversalAsync(IdempotencyRequest existingRequest, string requestHash);

        Task<Wallet> DoDestinationWalletExist(Guid walletId);
    }
}
