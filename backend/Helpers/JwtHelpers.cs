using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TitleVerification.Api.Helpers
{
    public class JwtHelper
    {
        private readonly IConfiguration _cfg;
        public JwtHelper(IConfiguration cfg) { _cfg = cfg; }

        public string GenerateToken(int userId, string username, string role = "User")
        {
            var key = _cfg["Jwt:Key"] ?? "super-secret-key-please-change";
            var issuer = _cfg["Jwt:Issuer"] ?? "TitleVerification";
            var audience = _cfg["Jwt:Audience"] ?? "TitleClient";
            var minutes = int.TryParse(_cfg["Jwt:ExpiryMinutes"], out var m) ? m : 120;

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddMinutes(minutes), signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
