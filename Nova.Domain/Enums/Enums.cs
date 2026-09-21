using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Domain.Enums
{
    public class Enums
    {
    }
    public enum WalletStatus
    {
        Active = 1,
        Suspended = 2,
        Frozen = 3,
        Closed = 4
    }

    public enum CustomerStatus
    {
        Active = 0,
        Suspended = 1,
        Locked = 2,
        Inactive = 3
    }


    public enum AuditAction
    {
        WalletCreated = 1,
        WalletCredited = 2,
        TransferCompleted = 3,
        TransferFailed = 4,
        WalletSuspended = 5,
        WalletFrozen = 6,
        WalletClosed = 7,
        WalletUnfrozen = 8,
        TransferReversed = 9
    }

    public enum TransactionType
    {
        Credit = 1,
        Debit = 2
    }

    public enum TransferStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Reversed = 4
    }

    public enum IdempotencyStatus
    {
        Processing = 1,
        Completed = 2,
        Failed = 3
    }

    public enum KycTier
    {
        Tier1 = 1,
        Tier2 = 2,
        Tier3 = 3
    }

    public enum OutboxMessageStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Reversed = 4
    }
}
