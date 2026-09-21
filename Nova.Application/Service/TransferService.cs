using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog;
using Nova.Application.Common;
using Nova.Application.Config;
using Nova.Application.Dto.Request;
using Nova.Application.Dto.Response;
using Nova.Application.Interfaces;
using Nova.Application.Validation;
using Nova.Domain.Entities;
using Nova.Domain.Enums;
using Nova.Domain.Exceptions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static Nova.Domain.Exceptions.NotFoundException;


namespace NovaWallet.Application.Services;

public class TransferService : ITransferService
{


    private readonly IUnitOfWork _unitOfWork;

    private readonly IMapper _mapper;

    private readonly IHelperService _helperService;

    private readonly IValidator<CreditWalletRequest> _validator;
    private readonly IValidator<TransferWalletRequest> _tranValidator;
    private readonly WalletConfig _walletSetting;
    private readonly Logger logger;

    

    public TransferService(
        IUnitOfWork unitOfWork,
        IMapper mapper,IHelperService helperService, IOptions<WalletConfig> walletSettings, IValidator<CreditWalletRequest> validator, IValidator<TransferWalletRequest> tranValidator)
    {
        _unitOfWork = unitOfWork;
        _walletSetting = walletSettings.Value;
        _mapper = mapper;
        _helperService = helperService;
        _validator = validator;
        _tranValidator = tranValidator;
        logger = LogManager.GetCurrentClassLogger();
    }
   
    public async Task<ResponseModel< CreditWalletResponse>> CreditAsync(Guid walletId,CreditWalletRequest request,Guid customerId,string idempotencyKey, string traceId)
    {
        logger.Info(
           "Wallet credit request started. " +
           "WalletId: {WalletId}, CustomerId: {CustomerId}, " +
           "IdempotencyKey: {IdempotencyKey}, TraceId: {TraceId}",
           walletId,
           customerId,
           idempotencyKey,
           traceId);

        await _validator.ValidateAndThrowAsync(request);

        ValidatorHelper.ValidateIdempotencyKey(idempotencyKey);

        var requestHash = LedgerHelpers.CreateRequestHash(walletId, request);

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            logger.Debug(
         "Credit transaction started. " +
         "WalletId: {WalletId}, TraceId: {TraceId}",
         walletId,
         traceId);

            await _unitOfWork.LockWalletAsync(walletId);

            var wallet = await _helperService.GetWalletAsync(walletId, customerId);

            ValidatorHelper.ValidateWallet(wallet);

            var existingRequest = await _helperService.GetIdempotencyRequestAsync(idempotencyKey);

            if (existingRequest != null)
            {
                var res = await _helperService.HandleExistingRequestAsync(existingRequest, requestHash);

                await _unitOfWork.SaveChangesAsync();

                return ResponseModel<CreditWalletResponse>.Ok("Credited successfully", res);

               
            }

            var transaction = LedgerHelpers.CreateTransaction(wallet, request);

            wallet.Balance = transaction.AmountBalanceAfterTransaction;

            var auditLog = LedgerHelpers.CreateAuditLog(nameof(Wallet), wallet.Id, AuditAction.WalletCredited, transaction.Amount, customerId.ToString(), traceId, metadata: new { ransactionId = transaction.Id, Reference = transaction.Reference, Description = request.Description });

            var idempotencyRecord = LedgerHelpers.CreateIdempotencyRecord(idempotencyKey, requestHash, wallet, transaction);

            logger.Debug(
               "Wallet credit transaction created. " +
               "TransactionId: {TransactionId}, WalletId: {WalletId}, " +
               "Amount: {Amount}, BalanceBefore: {BalanceBefore}, " +
               "BalanceAfter: {BalanceAfter}, TraceId: {TraceId}",
               transaction.Id,
               wallet.Id,
               transaction.Amount,
               transaction.AmountBalanceBeforeTransaction,
               transaction.AmountBalanceAfterTransaction,
               traceId);

            _unitOfWork.Repository<WalletTransaction>().AddAsync(transaction);

            _unitOfWork.Repository<AuditLog>().AddAsync(auditLog);

            _unitOfWork.Repository<IdempotencyRequest>().AddAsync(idempotencyRecord);

            await _unitOfWork.CommitTransactionAsync();
            await _unitOfWork.SaveChangesAsync();

            logger.Info(
           "Wallet credited successfully. " +
           "WalletId: {WalletId}, TransactionId: {TransactionId}, " +
           "Amount: {Amount}, BalanceAfter: {BalanceAfter}, " +
           "CustomerId: {CustomerId}, TraceId: {TraceId}",
           wallet.Id,
           transaction.Id,
           transaction.Amount,
           wallet.Balance,
           customerId,
           traceId);
            
            var response = _mapper.Map<CreditWalletResponse>(transaction);

            return ResponseModel<CreditWalletResponse>.Ok("Credited successfully", response);
        }
        catch (Exception ex)
        {
            {
                logger.Error(
                    ex,
                   "Wallet credit failed. Rolling back transaction. " +
                   "WalletId: {WalletId}, CustomerId: {CustomerId}, " +
                   "IdempotencyKey: {IdempotencyKey}, TraceId: {TraceId}",
                   walletId,
                   customerId,
                   idempotencyKey,
                   traceId);
                await _unitOfWork.RollbackTransactionAsync();

                throw;
            }
        }
    }


    public async Task< ResponseModel<TransferWalletResponse>> TransferAsync(
    Guid sourceWalletId,
    TransferWalletRequest request,
    Guid customerId,
    string idempotencyKey,
    string traceId)
    {
        logger.Info(
             "Wallet transfer request started. " +
             "SourceWalletId: {SourceWalletId}, " +
             "DestinationWalletId: {DestinationWalletId}, " +
             "CustomerId: {CustomerId}, " +
             "IdempotencyKey: {IdempotencyKey}, TraceId: {TraceId}",
             sourceWalletId,
             request.DestinationWalletId,
             customerId,
             idempotencyKey,
             traceId);



        await _tranValidator.ValidateAndThrowAsync(request);

       ValidatorHelper.ValidateIdempotencyKey(idempotencyKey);


        var requestHash =  LedgerHelpers.CreateRequestHash(
            sourceWalletId,
            request);

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            await _unitOfWork.LockWalletsAsync(sourceWalletId,request.DestinationWalletId);

           var sourceWallet = await _helperService.GetWalletAsync(sourceWalletId,customerId);


            var destinationWallet =await _helperService.DoDestinationWalletExist(request.DestinationWalletId);

            ValidatorHelper. ValidateTransferWallets(sourceWallet,destinationWallet,request.Amount);

            var existingRequest =await _helperService.GetIdempotencyRequestAsync(idempotencyKey);

            if (existingRequest != null)
            {
                var existingResponse =
                    await ValidatorHelper.HandleExistingTransferAsync(
                        existingRequest,
                        requestHash);

                await _unitOfWork.CommitTransactionAsync();

                return ResponseModel<TransferWalletResponse>.Ok("Transfer successfully",existingResponse);

                
            }

            await _helperService.CheckAndUpdateDailyTransferLimitAsync(sourceWallet.Id,request.Amount);
         

            var transfer = LedgerHelpers.CreateTransfer(sourceWallet,destinationWallet,request);

            logger.Info(
              "Transfer entity created. " +
              "TransferId: {TransferId}, Amount: {Amount}, " +
              "SourceWalletId: {SourceWalletId}, " +
              "DestinationWalletId: {DestinationWalletId}, " +
              "TraceId: {TraceId}",
              transfer.Id,
              transfer.Amount,
              sourceWallet.Id,
              destinationWallet.Id,
              traceId);

            var debitTransaction = LedgerHelpers.CreateDebitTransaction( sourceWallet, transfer);


            var creditTransaction =LedgerHelpers.CreateCreditTransaction(destinationWallet,transfer);

          
            sourceWallet.Balance =
         debitTransaction.AmountBalanceAfterTransaction;

          

            destinationWallet.Balance =
                creditTransaction.AmountBalanceAfterTransaction;
         

            var sourceAudit =LedgerHelpers.CreateAuditLog(
                entityType: nameof(WalletTransaction),
                entityId: debitTransaction.Id,
                action: AuditAction.TransferCompleted,
                amount: debitTransaction.Amount,
                actor: customerId.ToString(),
                traceId: traceId,
                metadata: new
                {
                    TransferId = transfer.Id,
                    WalletId = sourceWallet.Id,
                    SourceWalletId = sourceWallet.Id,
                    DestinationWalletId = destinationWallet.Id,
                    TransactionType = TransactionType.Debit.ToString(),
                    Reference = debitTransaction.Reference,
                    BalanceBefore = debitTransaction.AmountBalanceBeforeTransaction,
                    BalanceAfter = debitTransaction.AmountBalanceAfterTransaction,
                    
                });

         

            var destinationAudit =LedgerHelpers.CreateAuditLog(
                entityType: nameof(WalletTransaction),
                entityId: creditTransaction.Id,
                action: AuditAction.TransferCompleted,
                amount: creditTransaction.Amount,
                actor: customerId.ToString(),
                traceId: traceId,
                metadata: new
                {
                    TransferId = transfer.Id,
                    WalletId = destinationWallet.Id,
                    SourceWalletId = sourceWallet.Id,
                    DestinationWalletId = destinationWallet.Id,
                    TransactionType = TransactionType.Credit.ToString(),
                    Reference = creditTransaction.Reference,
                  
                 
                });

          

            var idempotencyRecord =LedgerHelpers.CreateTransferIdempotencyRecord(    idempotencyKey,  requestHash,  transfer);


            await _unitOfWork
                .Repository<Transfer>()
                .AddAsync(transfer);


            await _unitOfWork
                .Repository<WalletTransaction>()
                .AddAsync(debitTransaction);


            await _unitOfWork
                .Repository<WalletTransaction>()
                .AddAsync(creditTransaction);

        

            await _unitOfWork
                .Repository<AuditLog>()
                .AddAsync(sourceAudit);

           

            await _unitOfWork
                .Repository<AuditLog>()
                .AddAsync(destinationAudit);

         

            await _unitOfWork
                .Repository<IdempotencyRequest>()
                .AddAsync(idempotencyRecord);

            await _unitOfWork.CommitTransactionAsync();

            await _unitOfWork.SaveChangesAsync();

            logger.Info(
             "Wallet transfer completed successfully. " +
             "TransferId: {TransferId}, " +
             "SourceWalletId: {SourceWalletId}, " +
             "DestinationWalletId: {DestinationWalletId}, " +
             "Amount: {Amount}, " +
             "SourceBalanceAfter: {SourceBalanceAfter}, " +
             "DestinationBalanceAfter: {DestinationBalanceAfter}, " +
             "CustomerId: {CustomerId}, TraceId: {TraceId}",
             transfer.Id,
             sourceWallet.Id,
             destinationWallet.Id,
             transfer.Amount,
             sourceWallet.Balance,
             destinationWallet.Balance,
             customerId,
             traceId);

            var response =LedgerHelpers.CreateTransferResponse(
                transfer,
                sourceWallet,
                destinationWallet,
                debitTransaction,
                creditTransaction,
                traceId);

            
            return ResponseModel < TransferWalletResponse >.Ok("Transfer successful",response);
        }
        
        catch(Exception ex)
        {
            logger.Error(
                ex,
                "Wallet transfer failed. Rolling back transaction. " +
                "SourceWalletId: {SourceWalletId}, " +
                "DestinationWalletId: {DestinationWalletId}, " +
                "CustomerId: {CustomerId}, " +
                "IdempotencyKey: {IdempotencyKey}, " +
                "TraceId: {TraceId}",
                sourceWalletId,
                request.DestinationWalletId,
                customerId,
                idempotencyKey,
                traceId);
            await _unitOfWork.RollbackTransactionAsync();

            throw;
        }
    }


   
    

    
}