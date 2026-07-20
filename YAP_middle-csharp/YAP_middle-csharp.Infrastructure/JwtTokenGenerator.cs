using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using YAP_middle_csharp.Application.Interfaces;
using YAP_middle_csharp.Domain.Models;

namespace YAP_middle_csharp.Application.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(UserModel user)
        {
            var secret = _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var expiryMinutes = double.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new Dictionary<string, object>
            {
                { JwtRegisteredClaimNames.Sub, user.Id.ToString() },
                { JwtRegisteredClaimNames.UniqueName, user.Login },
                { ClaimTypes.Role, user.UserRole.ToString() } 
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Claims = claims,
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                SigningCredentials = credentials
            };

            var tokenHandler = new JsonWebTokenHandler();
            return tokenHandler.CreateToken(tokenDescriptor);
        }
    }
}
