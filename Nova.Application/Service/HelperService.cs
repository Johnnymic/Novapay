using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nova.Application.Common;
using Nova.Application.Config;
using Nova.Application.Dto.Response;
using Nova.Application.Dto.Response.Nova.Application.Dto.Response;
using Nova.Application.Interfaces;
using Nova.Domain.Entities;
using Nova.Domain.Enums;
using Nova.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nova.Domain.Exceptions.NotFoundException;

namespace Nova.Application.Service
{
    public class HelperService : IHelperService
    {

        private readonly IUnitOfWork _unitOfWork;

        private IMapper _mapper;

        private readonly WalletConfig _walletSettings;


        public HelperService(IUnitOfWork unitOfWork, IOptions<WalletConfig> walletSettings, IMapper mapper)
        {
            _unitOfWork= unitOfWork;
            _walletSettings = walletSettings.Value;
            _mapper= mapper;
        }


        public async Task<Wallet> DoesWalletExist(Guid customerId)
        {
            var wallet = await _unitOfWork.Repository<Wallet>().Query().AsNoTracking().FirstOrDefaultAsync(x => x.CustomerId == customerId);

            if (wallet == null)
            {
                throw new OperationFailedException(
                    "Wallet was not found.");
            }

            return wallet;
        }


        public async Task<Wallet> DoDestinationWalletExist(Guid walletId)
        {
            var wallet = await _unitOfWork.Repository<Wallet>().FirstOrDefaultAsync(x => x.Id == walletId);

            if (wallet == null)
            {
                throw new OperationFailedException(
                    "Wallet was not found.");
            }

            return wallet;
        }
        public async Task<Wallet> GetWalletAsync( Guid walletId,Guid customerId)
        {
            var wallet = await _unitOfWork
                .Repository<Wallet>()
                .FirstOrDefaultAsync(
                    x =>  x.Id == walletId &&
                        x.CustomerId == customerId);

            if (wallet == null)
            {
                throw new NotFoundException(
                    "Wallet was not found.");
            }

            return wallet;
        }


        public  async Task<IdempotencyRequest?>GetIdempotencyRequestAsync(   string idempotencyKey)
        {
            return await _unitOfWork
                .Repository<IdempotencyRequest>()
                .FirstOrDefaultAsync(
                    x => x.Key == idempotencyKey);
        }



        public async Task CheckAndUpdateDailyTransferLimitAsync(
        Guid walletId,
        decimal transferAmount)
        {
            
            var dailyLimit =
                _walletSettings.DailyTransferLimit;

     

            if (dailyLimit <= 0)
            {
                throw new OperationFailedException(
                    "Daily transfer limit is not configured correctly.");
            }

     

            if (walletId == Guid.Empty)
            {
                throw new OperationFailedException(
                    "Wallet ID is required.");
            }

         

            if (transferAmount <= 0)
            {
                throw new OperationFailedException(
                    "Transfer amount must be greater than zero.");
            }

   

            var businessDate =CommonUtils.
                GetNigeriaBusinessDate();


            var limitRepository =
                _unitOfWork.Repository<DailyTransferLimit>();

            var dailyLimitRecord =
                await limitRepository.FirstOrDefaultAsync(
                    x =>
                        x.WalletId == walletId &&
                        x.BusinessDate == businessDate);

            // =========================================================
            // 7. Create today's record if it doesn't exist
            // =========================================================

            if (dailyLimitRecord == null)
            {
                if (transferAmount > dailyLimit)
                {
                    throw new OperationFailedException(
                        $"Daily transfer limit of ₦{dailyLimit:N2} exceeded.");
                }

                dailyLimitRecord = new DailyTransferLimit
                {
                    Id = Guid.NewGuid(),

                    WalletId = walletId,

                    BusinessDate = businessDate,

                    LimitAmount = dailyLimit,

                    UsedAmount = transferAmount
                };

                await limitRepository.AddAsync(
                    dailyLimitRecord);

                return;
            }

            // =========================================================
            // 8. Calculate new usage
            // =========================================================

            var newUsedAmount =
                dailyLimitRecord.UsedAmount +
                transferAmount;

            // =========================================================
            // 9. Check daily limit
            // =========================================================

            if (newUsedAmount > dailyLimitRecord.LimitAmount)
            {
                var remainingAmount =
                    dailyLimitRecord.LimitAmount -
                    dailyLimitRecord.UsedAmount;

                throw new OperationFailedException(
                    $"Daily transfer limit exceeded. " +
                    $"Daily limit: ₦{dailyLimitRecord.LimitAmount:N2}. " +
                    $"Used: ₦{dailyLimitRecord.UsedAmount:N2}. " +
                    $"Remaining: ₦{Math.Max(remainingAmount, 0):N2}.");
            }

     

            dailyLimitRecord.UsedAmount =
                newUsedAmount;
        }


        public async Task<CreditWalletResponse>
            HandleExistingRequestAsync(
                IdempotencyRequest existingRequest,
                string requestHash)
        {
            if (!string.Equals(
                    existingRequest.RequestHash,
                    requestHash,
                    StringComparison.Ordinal))
            {
                throw new ConflictException(
                    "The Idempotency-Key has already been " +
                    "used with a different request.");
            }

            if (existingRequest.Id == null)
            {
                throw new OperationFailedException(
                    "The previous request has no transaction reference.");
            }

            var transaction = await _unitOfWork
                .Repository<WalletTransaction>()
                .FirstOrDefaultAsync(
                    x => x.Id == existingRequest.TransactionId);

            if (transaction == null)
            {
                throw new OperationFailedException(
                    "The original transaction could not be found.");
            }

            return _mapper.Map<CreditWalletResponse>(
                transaction);
        }

        public async Task<Transfer>ValidateTransferForReversal(Guid transferId)

        {
            var transfer = await _unitOfWork
                   .Repository<Transfer>()
                   .FirstOrDefaultAsync(x => x.Id == transferId);

            if (transfer == null)
            {
                throw new NotFoundException("Transfer was not found.");
            }

            if (transfer.Status == TransferStatus.Reversed)
            {
                throw new OperationFailedException("This transfer has already been reversed.");
            }

            //if (transfer.Status != TransferStatus.Completed)
            //{
            //    throw new OperationFailedException(
            //        $"Only completed transfers can be reversed. Current status: {transfer.Status}.");
            //}
            return transfer;

        }


        public async Task<ReverseTransferResponse> HandleExistingReversalAsync(IdempotencyRequest existingRequest, string requestHash)
        {
            if (!string.Equals(existingRequest.RequestHash, requestHash, StringComparison.Ordinal))
            {
                throw new ConflictException(
                    "The Idempotency-Key has already been used with a different reversal request.");
            }

            if (existingRequest.Status == IdempotencyStatus.Processing)
            {
                throw new OperationFailedException("The reversal request is already being processed.");
            }

            if (existingRequest.Status != IdempotencyStatus.Completed)
            {
                throw new OperationFailedException("The previous reversal request did not complete successfully.");
            }

            var reversalTransfer = await _unitOfWork
                .Repository<Transfer>()
                .FirstOrDefaultAsync(x => x.Id == existingRequest.TransactionId);

            if (reversalTransfer == null)
            {
                throw new OperationFailedException("The original reversal transfer could not be found.");
            }

            return new ReverseTransferResponse
            {
                OriginalTransferId = reversalTransfer.ReversalOfTransferId,
                ReversalTransferId = reversalTransfer.Id,
                SourceWalletId = reversalTransfer.DestinationWalletId,
                DestinationWalletId = reversalTransfer.SourceWalletId,
                Amount = reversalTransfer.Amount,
                Currency = "NGN",
                Reason = "Reversal (replayed from idempotency cache)",
                Status = reversalTransfer.Status.ToString(),
                CreatedAtUtc = reversalTransfer.CreatedAtUtc,
                TraceId = string.Empty
            };
        }



    }
}
