using Pic.Parametros;
using System.Security.Claims;

namespace Pic.Interface
{
    public interface IToken
    {
        string GenerateToken(UsuarioLoginDto usuario);
        string GenerateTokenConfirm(string Cache);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
