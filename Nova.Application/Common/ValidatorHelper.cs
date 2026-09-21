using Nova.Application.Dto.Response;
using Nova.Domain.Entities;
using Nova.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Nova.Domain.Exceptions.NotFoundException;

namespace Nova.Application.Common
{
    public static class ValidatorHelper
    {
        public const string NGN = "NGN";

        public static void ValidateTransferWallets(Wallet sourceWallet, Wallet destinationWallet, decimal amount)
        {
            if (amount <= 0)
                throw new OperationFailedException("Transfer amount must be greater than zero.");

            if (sourceWallet.Status != WalletStatus.Active)
                throw new OperationFailedException("Source wallet is not active.");

            if (destinationWallet.Status != WalletStatus.Active)
                throw new OperationFailedException("Destination wallet is not active.");

            if (sourceWallet.Id == destinationWallet.Id)
                throw new OperationFailedException("Source and destination wallets cannot be the same.");

            if (!string.Equals(sourceWallet.Currency, destinationWallet.Currency, StringComparison.OrdinalIgnoreCase))
                throw new OperationFailedException("Source and destination wallets must use the same currency.");

            if (sourceWallet.Balance < amount)
                throw new OperationFailedException("Insufficient wallet balance.");
        }


        public static void ValidateIdempotencyKey(string idempotencyKey)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
                throw new OperationFailedException("Idempotency-Key header is required.");

            if (idempotencyKey.Length > 200)
                throw new OperationFailedException("Idempotency-Key cannot exceed 200 characters.");
        }


        public static void ValidateWallet(Wallet wallet)
        {
            if (wallet.Status != WalletStatus.Active)
            {
                throw new OperationFailedException(
                    $"Wallet is not active. " +
                    $"Current status: {wallet.Status}.");
            }

            if (!string.Equals(
                    wallet.Currency,
                    NGN,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new OperationFailedException(
                    "Only NGN wallets are supported.");
            }
        }


        public static async Task<TransferWalletResponse> HandleExistingTransferAsync( IdempotencyRequest existingRequest, string requestHash)
        {
            // 1. Make sure the same idempotency key
            //    is not being used for a different request.
            if (!string.Equals(
                    existingRequest.RequestHash,
                    requestHash,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new OperationFailedException(
                    "The idempotency key has already been used for a different request.");
            }

            // 2. The request is still being processed.
            if (existingRequest.Status == IdempotencyStatus.Processing)
            {
                throw new OperationFailedException(
                    "The request is already being processed.");
            }

            // 3. The previous request failed.
            if (existingRequest.Status == IdempotencyStatus.Failed)
            {
                throw new OperationFailedException(
                    "The previous transfer request failed.");
            }

            // 4. We only return a stored response
            //    for a completed request.
            if (existingRequest.Status != IdempotencyStatus.Completed)
            {
                throw new OperationFailedException(
                    "Invalid idempotency request status.");
            }

            // 5. Make sure we actually stored the response.
            if (string.IsNullOrWhiteSpace(existingRequest.ResponseBody))
            {
                throw new OperationFailedException(
                    "The previous transfer response could not be found.");
            }

            // 6. Deserialize the original response.
            var response =
                JsonSerializer.Deserialize<TransferWalletResponse>(
                    existingRequest.ResponseBody);

            if (response == null)
            {
                throw new OperationFailedException(
                    "The previous transfer response could not be restored.");
            }

            return response;
        }


        public static void ValidateWallets(Wallet wallet)
        {
            if (wallet.Status == WalletStatus.Frozen)
            {
                throw new OperationFailedException("Wallet is already frozen.");
            }

            if (wallet.Status != WalletStatus.Active)
            {
                throw new OperationFailedException(
                    $"Only active wallets can be frozen. Current status: {wallet.Status}.");
            }
        }



    }
}
