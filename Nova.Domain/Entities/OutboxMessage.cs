using Nova.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Domain.Entities
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }

        public string EventType { get; set; } = null!;

        public string Payload { get; set; } = null!;

        public DateTime OccurredAtUtc { get; set; }

        public bool IsProcessed { get; set; }

        public DateTime? ProcessedAtUtc { get; set; }

        public int RetryCount { get; set; }

        public string? Error { get; set; }
        public OutboxMessageStatus Status { get; set; }
       
    }
}
