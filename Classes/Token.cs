using Microsoft.IdentityModel.Tokens;
using Pic.Interface;
using Pic.Tables;
using Pic.Tables.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pic.Classes
{
    public class Token : IToken
    {
        private readonly string Key;
        private readonly string Issuer;
        private readonly string Audience;
        private readonly string KeyConfirm;

        public Token(IConfiguration configuration)
        {;
            KeyConfirm = configuration["Bearer:KeyConfirm"];
            Key = configuration["Bearer:Key"];
            Issuer = configuration["Bearer:Issuer"];
            Audience = configuration["Bearer:Audience"];
        }

        public string GenerateToken(UsuarioLoginDto usuario)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("Id", usuario.Id.ToString()),
                new Claim("Nome", usuario.Nome),
                new Claim("Email", usuario.Email),
            };

            var token = new JwtSecurityToken(
                  issuer: Issuer,
                  audience: Audience,
                  claims: claims,
                  expires: DateTime.Now.AddHours(2),
                  signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateTokenConfirm(string Cache)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KeyConfirm));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("Cache", Cache),
            };

            var token = new JwtSecurityToken(
                  issuer: Issuer,
                  audience: Audience,
                  claims: claims,
                  expires: DateTime.Now.AddMinutes(15),
                  signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var keyBytes = Encoding.UTF8.GetBytes(KeyConfirm);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
