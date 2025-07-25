using KrystalAPI.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KrystalAPI.Services
{
    public class JwtService(IConfiguration config)
    {
        public string _secret = config["Jwt:Secret"]!;
        public string _issuer = config["Jwt:Issuer"]!;
        public string _audience = config["Jwt:Audience"]!;

        public ClaimsPrincipal RefTokenClaims;

        public string GenerateToken(User user)
        {
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("id", user.ID.ToString()),
                new Claim("role", user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                RoleClaimType = "role",
                NameClaimType = JwtRegisteredClaimNames.Sub
            };

            try
            {
                var claims = tokenHandler.ValidateToken(token, validationParams, out _);
                RefTokenClaims = claims;
                return claims;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            };
        }

        public TokenObj ReadJwt(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
            var claims = new ClaimsPrincipal(identity);
            return ToTokenObj(claims);
        }

        public TokenObj ToTokenObj(ClaimsPrincipal tokenClaims)
        {
            if (tokenClaims == null) return null;
            var claims = tokenClaims.Claims;

            return new TokenObj
            {
                sub = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value,
                id = claims.FirstOrDefault(c => c.Type == "id")?.Value,
                role = Enum.TryParse<UserRole>(claims.FirstOrDefault(c => c.Type == "role")?.Value, out var role) ? role : UserRole.User,
                jti = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value,
                exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value ?? "0")).UtcDateTime,
                iss = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iss)?.Value,
                aud = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Aud)?.Value
            };
        }
    }
}
