using DvizhX.Application.Common.Interfaces.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DvizhX.Infrastructure.Authentication
{
    public class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
    {
        public (string AccessToken, string Jti) GenerateAccessToken(Guid userId, string username, string email)
        {
            var secretKey = configuration["JwtSettings:Secret"]
                            ?? throw new InvalidOperationException("JwtSettings:Secret is not configured.");

            // Ключ должен быть минимум 256 бит (32 символа)
            if (Encoding.UTF8.GetByteCount(secretKey) < 32)
            {
                throw new InvalidOperationException("JwtSettings:Secret is too short. It must be at least 32 characters.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jti = Guid.NewGuid().ToString(); // Unique ID for this specific token

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, jti)
        };

            var issuer = configuration["JwtSettings:Issuer"];
            var audience = configuration["JwtSettings:Audience"];

            // Парсим время жизни токена (в минутах), по дефолту 15 минут
            var expiryMinutesStr = configuration["JwtSettings:ExpiryMinutes"];
            var expiryMinutes = int.TryParse(expiryMinutesStr, out var val) ? val : 15;

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), jti);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
