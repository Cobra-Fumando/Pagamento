using Microsoft.IdentityModel.Tokens;
using Pic.Parametros;
using Pic.Tables;
using System.IdentityModel.Tokens.Jwt;

namespace Pic.Classes
{
    public class Token
    {
        private readonly string Key;
        private readonly string Issuer;
        private readonly string Audience;

        public Token(IConfiguration configuration)
        {
            Key = configuration["Bearer:Key"];
            Issuer = configuration["Bearer:Issuer"];
            Audience = configuration["Bearer:Audience"];
        }

        public string GenerateToken(UsuarioLoginDto usuario)
        {
            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new System.Security.Claims.Claim("Id", usuario.Id.ToString()),
                new System.Security.Claims.Claim("Nome", usuario.Nome),
                new System.Security.Claims.Claim("Email", usuario.Email),
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
    }
}
