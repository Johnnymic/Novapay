using Microsoft.Extensions.Configuration;
using Nova.Application.Interfaces;
using Nova.Application.Interfaces.Auth;
using Nova.Domain.Entities;
using Microsoft.EntityFrameworkCore; 
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Nova.Application.Service.Auth
{
    public class TokenService : ITokenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public TokenService(
            IUnitOfWork unitOfWork,
            IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _config = config;
        }

        public async Task<string> GenerateTokenAsync( Guid userId, string Email)
        {
            var userRepo = _unitOfWork.Repository<Customer>();

            var user = await userRepo
                .Query()
                .Where(u =>
                   
                    u.Id == userId &&
                    u.Email == Email)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.Role
                    
                    
                   
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new UnauthorizedAccessException(
                    "User could not be found.");
            }

           

            var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

              new(
                    "customerId",
                    user.Id.ToString()),

            new(
                ClaimTypes.Name,
                user.FirstName),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email),

           new(
                    ClaimTypes.Role,
                    user.Role.ToString()),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _config["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var expiresIn = int.Parse(
                _config["Jwt:ExpiresIn"]!);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresIn),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}
