using Nova.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Domain.Entities
{
    public class Customer
    {
      

        public Guid Id { get; set; }

        public string CustomerReference { get; set; } 

        public string FirstName { get; set; } 

        public string LastName { get; set; } 

        public string PhoneNumber { get; set; }

        public string PasswordHash { get; set; }

        public string Email { get; set; } 

        public string Bvn { get; set; }

        public string Nin { get; set; }

        public string Role { get; set; }

        public KycTier KycTier { get; set; }

        public CustomerStatus Status { get; set; } 

        public DateTime CreatedAt { get; set; }

        public ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
    }
}
