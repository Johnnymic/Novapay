using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nova.Application.Interfaces;
using Nova.DataAccess.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.DataAccess.Extension
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddDataAccessServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<NovaWalletDbContext>(options =>
            {
                var connectionString = config.GetConnectionString("DefaultConnection");

                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString));
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }
    }
}
