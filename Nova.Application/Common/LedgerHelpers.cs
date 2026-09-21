using Nova.Application.Dto.Request;
using Nova.Application.Dto.Response;
using Nova.Domain.Entities;
using Nova.Domain.Enums;
using Nova.Domain.Exceptions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static Nova.Domain.Exceptions.NotFoundException;
using static NovaWallet.Application.Services.WalletService;

namespace Nova.Application.Common
{
    /// <summary>
    /// Stateless building blocks shared between WalletService (customer-facing
    /// money movement: credit, transfer) and AdminService (admin-facing wallet
    /// actions: freeze, unfreeze, reverse). Extracted here so both services
    /// create WalletTransaction/AuditLog rows and validate wallets identically —
    /// without this, AdminService.ReverseTransferAsync would need its own copy
    /// of logic that must always match WalletService's, which is exactly the
    /// kind of duplication that drifts out of sync over time.
    /// </summary>
    public static class LedgerHelpers
    {
        public static AuditLog CreateAuditLog(
            string entityType,
            Guid entityId,
            AuditAction action,
            decimal? amount,
            string? actor,
            string traceId,
            object? metadata = null)
        {
            return new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = entityId,
                Action = action.ToString(),
                Amount = amount,
                Actor = actor,
                TraceId = traceId,
                Metadata = metadata == null ? null : JsonSerializer.Serialize(metadata),
                CreatedAt = DateTime.UtcNow
            };
        }

        public static WalletTransaction CreateDebitTransactions(Wallet sourceWallet, Transfer transfer)
        {
            var balanceBefore = sourceWallet.Balance;
            var balanceAfter = balanceBefore - transfer.Amount;

            return new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = sourceWallet.Id,
                TransferId = transfer.Id,
                Type = TransactionType.Debit,
                Amount = transfer.Amount,
                AmountBalanceBeforeTransaction = balanceBefore,
                AmountBalanceAfterTransaction = balanceAfter,
                Reference = $"{transfer.Reference}-DR",
                CreatedAtUtc = DateTime.UtcNow
            };
        }

        public static WalletResponse ToWalletResponse(Wallet wallet)
        {
            return new WalletResponse
            {
                walletId = wallet.Id,
                CustomerId = wallet.CustomerId,
                Currency = wallet.Currency,
                Balance = wallet.Balance,
                Status = wallet.Status.ToString(),
                CreatedAt = wallet.CreatedAtUtc
            };
        }
        public static WalletTransaction CreateCreditTransaction(Wallet destinationWallet, Transfer transfer)
        {
            var balanceBefore = destinationWallet.Balance;
            var balanceAfter = balanceBefore + transfer.Amount;

            return new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = destinationWallet.Id,
                TransferId = transfer.Id,
                Type = TransactionType.Credit,
                Amount = transfer.Amount,
                AmountBalanceBeforeTransaction = balanceBefore,
                AmountBalanceAfterTransaction = balanceAfter,
                Reference = $"{transfer.Reference}-CR",
                CreatedAtUtc = DateTime.UtcNow
            };
        }

        public static WalletTransaction CreateTransaction( Wallet wallet, CreditWalletRequest request)
        {
            var balanceBefore = wallet.Balance;

            var balanceAfter =
                balanceBefore + request.Amount;

            return new WalletTransaction
            {
                Id = Guid.NewGuid(),

                WalletId = wallet.Id,

                Type = TransactionType.Credit,

                Amount = request.Amount,

                AmountBalanceBeforeTransaction = balanceBefore,

                AmountBalanceAfterTransaction = balanceAfter,

                Reference = request.Reference,

                CreatedAtUtc = DateTime.UtcNow
            };
        }


        public static IdempotencyRequest
            CreateIdempotencyRecord(
                string key,
                string requestHash,
                Wallet wallet,
                WalletTransaction transaction)
        {
            return new IdempotencyRequest
            {
                Id = Guid.NewGuid(),

                Key = key,

                RequestHash = requestHash,

                Operation = "",

                WalletId = wallet.Id,

                TransactionId = transaction.Id,

                Status = IdempotencyStatus.Completed,

                CreatedAtUtc = DateTime.UtcNow
            };
        }


        public static Transfer CreateTransfer(
        Wallet sourceWallet,
        Wallet destinationWallet,
        TransferWalletRequest request)
        {
            return new Transfer
            {
                Id = Guid.NewGuid(),

                SourceWalletId = sourceWallet.Id,

                DestinationWalletId = destinationWallet.Id,

                Amount = request.Amount,

                Reference = request.Reference,


                Status = TransferStatus.Completed,

                CreatedAtUtc = DateTime.UtcNow
            };
        }

        public static TransferWalletResponse CreateTransferResponse(
   Transfer transfer,
   Wallet sourceWallet,
   Wallet destinationWallet,
   WalletTransaction debitTransaction,
   WalletTransaction creditTransaction,
   string traceId)
        {
            return new TransferWalletResponse
            {
                TransferId = transfer.Id,

                SourceWalletId = sourceWallet.Id,

                DestinationWalletId = destinationWallet.Id,

                Amount = transfer.Amount,

                SourceBalanceBefore =
                    debitTransaction.AmountBalanceBeforeTransaction,

                SourceBalanceAfter =
                    debitTransaction.AmountBalanceAfterTransaction,

                DestinationBalanceBefore =
                    creditTransaction.AmountBalanceBeforeTransaction,

                DestinationBalanceAfter =
                    creditTransaction.AmountBalanceAfterTransaction,

                Currency = sourceWallet.Currency,

                Reference = transfer.Reference,

                Status = transfer.Status.ToString(),

                CreatedAtUtc = transfer.CreatedAtUtc,

                TraceId = traceId
            };
        }


        public static IdempotencyRequest CreateTransferIdempotencyRecord(string key,
       string requestHash,
       Transfer transfer)
        {
            return new IdempotencyRequest
            {
                Id = Guid.NewGuid(),

                Key = key,

                RequestHash = requestHash,

                Operation = "WalletTransfer",

                WalletId = transfer.SourceWalletId,

                TransactionId = transfer.Id,

                Status = IdempotencyStatus.Completed,

                CreatedAtUtc = DateTime.UtcNow
            };
        }

        public static WalletTransaction CreateDebitTransaction(
   Wallet sourceWallet,
   Transfer transfer)
        {
            var balanceBefore = sourceWallet.Balance;

            var balanceAfter =
                balanceBefore - transfer.Amount;

            return new WalletTransaction
            {
                Id = Guid.NewGuid(),

                WalletId = sourceWallet.Id,

                TransferId = transfer.Id,

                Type = TransactionType.Debit,

                Amount = transfer.Amount,

                AmountBalanceBeforeTransaction = balanceBefore,

                AmountBalanceAfterTransaction = balanceAfter,

                Reference = $"{transfer.Reference}-DR",

                CreatedAtUtc = DateTime.UtcNow
            };
        }



        public static string CreateRequestHash(
   Guid sourceWalletId,
   TransferWalletRequest request)
        {
            var payload = string.Join(
                "|",
                sourceWalletId,
                request.DestinationWalletId,
                request.Amount,
                request.Reference,
                request.Description ?? string.Empty);

            return CommonUtils.GenerateHash(
                payload);
        }

        public static string CreateRequestHash(
      Guid walletId,
      CreditWalletRequest request)
        {
            var payload = string.Join(
                "|",
                walletId,
                request.Amount,
                request.Reference,
                request.Description ?? string.Empty);

            return CommonUtils.GenerateHash(
                payload);
        }


        public static (int Page, int PageSize) NormalizePagination(
    WalletStatementRequest request)
        {
            var page = request.Page <= 0
                ? 1
                : request.Page;

            var pageSize = request.PageSize <= 0
                ? 20
                : Math.Min(request.PageSize, 100);

            return (page, pageSize);
        }

        public static IQueryable<WalletTransaction> ApplyStatementFilters(
    IQueryable<WalletTransaction> query,
    WalletStatementRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Type) &&
                Enum.TryParse<TransactionType>(
                    request.Type,
                    true,
                    out var typeFilter))
            {
                query = query.Where(x => x.Type == typeFilter);
            }

            if (request.From.HasValue)
            {
                query = query.Where(x =>
                    x.CreatedAtUtc >= request.From.Value);
            }

            if (request.To.HasValue)
            {
                query = query.Where(x =>
                    x.CreatedAtUtc <= request.To.Value);
            }

            return query;
        }


        public static IQueryable<StatementEntryResponse> ProjectStatementEntries(  IQueryable<WalletTransaction> query)
        {
            return query.Select(x => new StatementEntryResponse
            {
                TransactionId = x.Id,
                TransferId = x.TransferId,
                Type = x.Type.ToString(),
                Amount = x.Amount,
                BalanceBefore = x.AmountBalanceBeforeTransaction,
                BalanceAfter = x.AmountBalanceAfterTransaction,
                Reference = x.Reference,
                CreatedAtUtc = x.CreatedAtUtc
            });
        }








    }
}