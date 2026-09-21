using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nova.Application.Dto.Request;
using Nova.Application.Dto.Response;
using Nova.Application.Interfaces;
using Nova.Application.Interfaces.Auth;
using Nova.Domain.Entities;
using Nova.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Service.Auth
{
   

    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher<Customer> _passwordHasher;
        private readonly IMapper _mapper;

        public AuthService(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IPasswordHasher<Customer> passwordHasher, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
        }

        public async Task<ResponseModel< string>> RegisterAsync(RegisterRequest request)
        {

            var customerRepository =
                _unitOfWork.Repository<Customer>();

            var walletRepository =
                _unitOfWork.Repository<Wallet>();

      
            var email = request.Email
                .Trim()
                .ToLowerInvariant();

          
            var emailExists = await customerRepository
                .Query().AnyAsync(
                    x => x.Email == email);

            if (emailExists)
            {
                throw new InvalidOperationException(
                    "A customer with this email already exists.");
            }

           
            var customer = _mapper.Map<Customer>(request);
            customer.Id = Guid.NewGuid();
            customer.CustomerReference = $"CUS-{Guid.NewGuid():N}".ToUpper();
            customer.Email = email;
            customer.Role = "Customer";
            customer.Status =CustomerStatus.Active;
            customer.KycTier = KycTier.Tier1;
            customer.PasswordHash = _passwordHasher.HashPassword(customer, request.Password);

            await customerRepository.AddAsync(customer);

          
            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),

                CustomerId = customer.Id,

                Currency = "NGN",

                Balance = 0.00m,

                Status = WalletStatus.Active,

                CreatedAtUtc = DateTime.UtcNow
            };

            await walletRepository.AddAsync(wallet);

           
            await _unitOfWork.SaveChangesAsync();

         
            return ResponseModel<string>.Ok("Registered successfuly",null);
        }

        public async Task<ResponseModel<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var customerRepository =
                _unitOfWork.Repository<Customer>();

            // 1. Normalize email
            var email = request.Email
                .Trim()
                .ToLowerInvariant();

            // 2. Find customer
            var customer = await customerRepository
                .Query()
                .FirstOrDefaultAsync(
                    x => x.Email == email);

            if (customer == null)
            {
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }

            // 3. Check account status
            if (customer.Status != CustomerStatus.Active)
            {
                throw new UnauthorizedAccessException(
                    "Customer account is not active.");
            }

            // 4. Verify password
            var passwordResult =
                _passwordHasher.VerifyHashedPassword(
                    customer,
                    customer.PasswordHash,
                    request.Password);

            if (passwordResult ==
                PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }

         
            var token =
                await _tokenService.GenerateTokenAsync(
                    customer.Id,
                    customer.Email);

            var expiresAtUtc =
                DateTime.UtcNow.AddMinutes(30);

             var login=new LoginResponse
            {
                AccessToken = token,

                ExpiresAtUtc = expiresAtUtc
            };

            return ResponseModel<LoginResponse>.Ok("login  successfuly", login);
        }
    }
}
