using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ParImparAPI.Domain.Services
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;

        // Credenciales fijas
        private const string FIXED_USERNAME = "admin";
        private const string FIXED_PASSWORD = "123456";

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool ValidateCredentials(string username, string password)
        {
            return username == FIXED_USERNAME && password == FIXED_PASSWORD;
        }

        public string GenerateJwtToken(string username)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKey12345678901234567890"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "ParImparAPI",
                audience: _configuration["Jwt:Audience"] ?? "ParImparAPI",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24), // Token válido por 24 horas
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
} 