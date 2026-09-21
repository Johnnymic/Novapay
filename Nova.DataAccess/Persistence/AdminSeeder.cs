using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nova.Domain.Entities;
using Nova.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.DataAccess.Persistence
{

    namespace Nova.DataAccess.Seed
    {
        public static class AdminSeeder
        {
            public static async Task SeedAsync(
                NovaWalletDbContext context,
                IPasswordHasher<Customer> passwordHasher)
            {
                // Check whether an admin already exists
                var adminExists = await context.Customers.AnyAsync(x => x.Role == "Admin");

                if (adminExists)
                {
                    return;
                }

                var admin = new Customer
                {
                    Id = Guid.NewGuid(),

                    FirstName = "NovaPay",

                    LastName = "Administrator",

                    Email = "admin@novapay.com",

                    Bvn = null,

                    Role = "Admin",

                    PhoneNumber="",

                    CustomerReference="",

                    Status = CustomerStatus.Active,

                    CreatedAt = DateTime.UtcNow
                };

                
                admin.PasswordHash = passwordHasher.HashPassword(
                    admin,
                    "Admin@12345"
                );

                await context.Customers.AddAsync(admin);

                await context.SaveChangesAsync();
            }
        }
    }
}
