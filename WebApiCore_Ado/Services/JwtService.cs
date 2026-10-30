using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebApiCore_Ado.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string CreateToken(string userId, string role)
        {
            var claim = new[]
            {
                new Claim("userId" , userId),
                new Claim(ClaimTypes.Role , role),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claim,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: cred
                );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}
