using Microsoft.EntityFrameworkCore;
using NLog;
using Nova.Application.Common;
using Nova.Application.Dto.Request;
using Nova.Application.Dto.Response;
using Nova.Application.Dto.Response.Nova.Application.Dto.Response;
using Nova.Application.Interfaces;
using Nova.Domain.Entities;
using Nova.Domain.Enums;
using Nova.Domain.Exceptions;
using System.Security.Cryptography.Xml;
using static Nova.Domain.Exceptions.NotFoundException;

namespace Nova.Application.Service
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IHelperService _helperService;

        private readonly Logger logger;
        public AdminService(IUnitOfWork unitOfWork, IHelperService helperService)
        {
            _unitOfWork = unitOfWork;
            _helperService = helperService;
            logger = LogManager.GetCurrentClassLogger();
        }





        public async Task<ResponseModel<List<AdminCustomerWalletResponse>>>
      GetAllCustomersWithWalletsAsync(string traceId)
        {
            logger.Info("Admin requested all customers and wallets. TraceId: {TraceId}",traceId);

            var customers = await _unitOfWork
                .Repository<Customer>()
                .Query()
                .AsNoTracking()
                .Include(x => x.Wallets)
                .ToListAsync();



            var customerIds = customers
                .Select(x => x.Id)
                .ToList();

            var walletIds = customers
                .SelectMany(x => x.Wallets)
                .Select(x => x.Id)
                .ToList();

            var transfers = await _unitOfWork
                    .Repository<Transfer>()
                    .Query()
                    .AsNoTracking()
                    .Where(x =>
                        walletIds.Contains(x.SourceWalletId) ||
                        walletIds.Contains(x.DestinationWalletId))
                    .OrderByDescending(x => x.CreatedAtUtc)
                    .ToListAsync();

            logger.Info(
                 "Wallet transfers retrieved successfully. " +
                 "TransferCount: {TransferCount}, TraceId: {TraceId}",
                 transfers.Count,
                 traceId);

            var response = customers.Select(customer => new AdminCustomerWalletResponse
            {
                CustomerId = customer.Id,
                CustomerReference = customer.CustomerReference,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email,

                Wallets = customer.Wallets.Select(wallet =>
                       new AdminWalletResponse
                       {
                           WalletId = wallet.Id,
                           Balance = wallet.Balance,
                           Currency = wallet.Currency,
                           Status = wallet.Status.ToString(),

                           Transfers = transfers
                               .Where(x =>
                                   x.SourceWalletId == wallet.Id ||
                                   x.DestinationWalletId == wallet.Id)
                               .Select(x => new AdminTransferResponse
                               {
                                   TransferId = x.Id,
                                   SourceWalletId = x.SourceWalletId,
                                   DestinationWalletId = x.DestinationWalletId,
                                   Amount = x.Amount,
                                   Reference = x.Reference,
                                   Status = x.Status.ToString(),
                                   CreatedAtUtc = x.CreatedAtUtc,
                                   CompletedAtUtc = x.CompletedAtUtc,
                                   ReversalOfTransferId = x.ReversalOfTransferId
                               })
                               .ToList()
                       })
                  .ToList()
            }).ToList();

            logger.Info(
                "Admin customers and wallets response built successfully. " +
                "CustomerCount: {CustomerCount}, TraceId: {TraceId}",
                response.Count,
                traceId);

             return ResponseModel<List<AdminCustomerWalletResponse>>.Ok("Customers and wallets retrieved successfully.",response);

        }




        public async Task <ResponseModel< WalletResponse>> FreezeWalletAsync(
            Guid walletId,
            FreezeWalletRequest request,
            string actor,
            string traceId)
        {

             logger.Info(
                    "Admin wallet freeze request started. " +
                    "WalletId: {WalletId}, Actor: {Actor}, " +
                    "Request: {@Request}, TraceId: {TraceId}",walletId, actor, request, traceId);

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                throw new OperationFailedException("A reason is required to freeze a wallet.");
            }

            await _unitOfWork.BeginTransactionAsync();

            logger.Debug(
             "Wallet freeze transaction started. " +
             "WalletId: {WalletId}, TraceId: {TraceId}",
             walletId,
             traceId);

            try
            {
                await _unitOfWork.LockWalletAsync(walletId);

                var wallet = await _helperService.DoDestinationWalletExist(walletId);

                ValidatorHelper.ValidateWallets(wallet);

                wallet.Status = WalletStatus.Frozen;

                var auditLog = LedgerHelpers.CreateAuditLog(
                    entityType: nameof(Wallet),
                    entityId: wallet.Id,
                    action: AuditAction.WalletFrozen,
                    amount: null,
                    actor: actor,
                    traceId: traceId,
                    metadata: new { Reason = request.Reason });

               

                await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog);

                await _unitOfWork.CommitTransactionAsync();
                await _unitOfWork.SaveChangesAsync();

                logger.Debug(
                "Wallet freeze audit log created. " +
                "WalletId: {WalletId}, Actor: {Actor}, TraceId: {TraceId}",
                wallet.Id,
                actor,
                traceId);

                var response = LedgerHelpers.ToWalletResponse(wallet);

                return ResponseModel<WalletResponse>.Ok("",response) ;
            }
            catch
            {
                logger.Error("Wallet freeze failed. Rolling back transaction. " +
                    "WalletId: {WalletId}, Actor: {Actor}, TraceId: {TraceId}",
                    walletId,
                    actor,
                    traceId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        

        public async Task<ResponseModel<WalletResponse>> UnfreezeWalletAsync(
            Guid walletId,
            UnfreezeWalletRequest request,
            string actor,
            string traceId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _unitOfWork.LockWalletAsync(walletId);

                var wallet = await _helperService.DoDestinationWalletExist(walletId);

                if (wallet.Status != WalletStatus.Frozen)
                {
                    throw new OperationFailedException(
                        $"Only frozen wallets can be unfrozen. Current status: {wallet.Status}.");
                }

                wallet.Status = WalletStatus.Active;

                var auditLog = LedgerHelpers.CreateAuditLog(
                    entityType: nameof(Wallet),
                    entityId: wallet.Id,
                    action: AuditAction.WalletUnfrozen,
                    amount: null,
                    actor: actor,
                    traceId: traceId,
                    metadata: new { Reason = request.Reason });

                await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog);

                await _unitOfWork.CommitTransactionAsync();
                await _unitOfWork.SaveChangesAsync();

                var response = LedgerHelpers. ToWalletResponse(wallet);

                return ResponseModel<WalletResponse>.Ok("successfuly", response);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

       

        public async Task<ResponseModel< ReverseTransferResponse>> ReverseTransferAsync(
            Guid transferId,
            ReverseTransferRequest request,
            string actor,
            string idempotencyKey,
            string traceId)
        {
            ValidatorHelper.ValidateIdempotencyKey(idempotencyKey);


            var requestHash = CommonUtils.GenerateHash(string.Join("|", transferId, request.Reason));

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingRequest = await _helperService.GetIdempotencyRequestAsync(idempotencyKey);

                if (existingRequest != null)
                {
                    var replay = await _helperService.HandleExistingReversalAsync(existingRequest, requestHash);
                    await _unitOfWork.CommitTransactionAsync();
                    return ResponseModel<ReverseTransferResponse>.Ok("successfuly",replay);
                }

                
              var transfer = await _helperService.ValidateTransferForReversal(transferId);

                 await _unitOfWork.LockWalletsAsync(transfer.SourceWalletId, transfer.DestinationWalletId);

                var originalSourceWallet = await _helperService.DoDestinationWalletExist(transfer.SourceWalletId);
                var originalDestinationWallet = await _helperService.DoDestinationWalletExist(transfer.DestinationWalletId);

                ValidatorHelper.ValidateTransferWallets(originalDestinationWallet, originalSourceWallet,transfer.Amount);

                var reversalTransfer = new Transfer
                {
                    Id = Guid.NewGuid(),
                    SourceWalletId = originalDestinationWallet.Id,
                    DestinationWalletId = originalSourceWallet.Id,
                    Amount = transfer.Amount,
                    Reference = $"{transfer.Reference}-REV",
                    Status = TransferStatus.Completed,
                    ReversalOfTransferId = transfer.Id,
                    CreatedAtUtc = DateTime.UtcNow
                };

                var debitTransaction = LedgerHelpers.CreateDebitTransaction(originalDestinationWallet, reversalTransfer);
                var creditTransaction = LedgerHelpers.CreateCreditTransaction(originalSourceWallet, reversalTransfer);

                originalDestinationWallet.Balance = debitTransaction.AmountBalanceAfterTransaction;
                originalSourceWallet.Balance = creditTransaction.AmountBalanceAfterTransaction;

                transfer.Status = TransferStatus.Reversed;
                transfer.ReversalOfTransferId = reversalTransfer.Id;

                var debitAudit = LedgerHelpers.CreateAuditLog(
                    entityType: nameof(Transfer),
                    entityId: transfer.Id,
                    action: AuditAction.TransferReversed,
                    amount: transfer.Amount,
                    actor: actor,
                    traceId: traceId,
                    metadata: new
                    {
                        OriginalTransferId = transfer.Id,
                        ReversalTransferId = reversalTransfer.Id,
                        WalletId = originalDestinationWallet.Id,
                        Direction = "Debit",
                        Reason = request.Reason
                    });

                var creditAudit = LedgerHelpers.CreateAuditLog(
                    entityType: nameof(Transfer),
                    entityId: transfer.Id,
                    action: AuditAction.TransferReversed,
                    amount: transfer.Amount,
                    actor: actor,
                    traceId: traceId,
                    metadata: new
                    {
                        OriginalTransferId = transfer.Id,
                        ReversalTransferId = reversalTransfer.Id,
                        WalletId = originalSourceWallet.Id,
                        Direction = "Credit",
                        Reason = request.Reason
                    });

                var idempotencyRecord = new IdempotencyRequest
                {
                    Id = Guid.NewGuid(),
                    Key = idempotencyKey,
                    RequestHash = requestHash,
                    Operation = "WalletTransferReversal",
                    WalletId = originalSourceWallet.Id,
                    TransactionId = reversalTransfer.Id,
                    Status = IdempotencyStatus.Completed,
                    CreatedAtUtc = DateTime.UtcNow
                };

                await _unitOfWork.Repository<Transfer>().AddAsync(reversalTransfer);
                await _unitOfWork.Repository<WalletTransaction>().AddAsync(debitTransaction);
                await _unitOfWork.Repository<WalletTransaction>().AddAsync(creditTransaction);
                await _unitOfWork.Repository<AuditLog>().AddAsync(debitAudit);
                await _unitOfWork.Repository<AuditLog>().AddAsync(creditAudit);
                await _unitOfWork.Repository<IdempotencyRequest>().AddAsync(idempotencyRecord);

                await _unitOfWork.CommitTransactionAsync();
                await _unitOfWork.SaveChangesAsync();

                var reponse = new ReverseTransferResponse
                {
                    OriginalTransferId = transfer.Id,
                    ReversalTransferId = reversalTransfer.Id,
                    SourceWalletId = originalSourceWallet.Id,
                    DestinationWalletId = originalDestinationWallet.Id,
                    Amount = transfer.Amount,
                    Currency = originalSourceWallet.Currency,
                    Reason = request.Reason,
                    Status = reversalTransfer.Status.ToString(),
                    CreatedAtUtc = reversalTransfer.CreatedAtUtc,
                    TraceId = traceId
                };

                return ResponseModel<ReverseTransferResponse>.Ok("Reversed successfuly",reponse);

            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

  

      

        


      

      

      
    }
}