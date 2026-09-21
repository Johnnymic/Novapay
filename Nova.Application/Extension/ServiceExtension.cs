using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nova.Application.Config;
using Nova.Application.Dto.Request;
using Nova.Application.Interfaces;
using Nova.Application.Interfaces.Auth;
using Nova.Application.Service;
using Nova.Application.Service.Auth;
using Nova.Application.Validation;
using Nova.Domain.Entities;
using NovaWallet.Application.Services;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Text;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;


namespace Nova.Application.Extension
{
    public  static class ServiceExtension
    {

        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());
           services.AddScoped<IHelperService, HelperService>();
            services.AddTransient <IAuthService, AuthService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<ITransferService, TransferService>();
            services.AddScoped<ITokenService,TokenService>();
            services.AddScoped<IPasswordHasher<Customer>, PasswordHasher<Customer>>();
            services.Configure<WalletConfig>(configuration.GetSection("WalletSettings"));


            services.TryAddScoped<IValidator<CreditWalletRequest>, CreditWalletRequestValidator>();
            services.TryAddScoped<IValidator<TransferWalletRequest>, TransferWalletRequestValidator>();
            return services;
        }

        public static IServiceCollection AddAppSwagger(this IServiceCollection services, IConfiguration config)
        {


            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "NovaPay  API",
                    Version = "v1",
                    Description = "API for NovaPay"
                });


                // Enables [SwaggerOperation], [SwaggerResponse], etc. from Swashbuckle.AspNetCore.Annotations
                options.EnableAnnotations();

                // Enables XML doc comments (/// <summary>) to show up as descriptions
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
 .AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,

         ValidIssuer = config["JWT:Issuer"],
         ValidAudience = config["JWT:Audience"],

         IssuerSigningKey = new SymmetricSecurityKey(
             Encoding.UTF8.GetBytes(
                 config["JWT:Key"]!)),

         ClockSkew = TimeSpan.Zero
     };
 });

            return services;
        }

        
    }
}
