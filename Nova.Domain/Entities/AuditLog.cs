using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        public string EntityType { get; set; } = null!;

        public Guid EntityId { get; set; }

        public string Action { get; set; } = null!;

        public decimal? Amount { get; set; }

        public string? Actor { get; set; }

      

        public string? Metadata { get; set; }

        public DateTime CreatedAt { get; set; }
        public string TraceId { get; set; }
    }
}
