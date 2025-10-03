using Pic.Classes;
using Pic.Parametros;
using Pic.Tables;

namespace Pic.Condicao
{
    public class CriarUser
    {
        public static TabelaProblem<string> ValidarCodicao(UsuarioDto usuarioDto)
        {
            if(usuarioDto.Nome.Length < 3) return StatusProblem.Fail<string>("Nome muito pequeno");
            if(usuarioDto.Senha.Length < 5) return StatusProblem.Fail<string>("Senha muito pequena");
            if (!EmailVerify.IsValidEmail(usuarioDto.Email)) return StatusProblem.Fail<string>("Email inválido");

            return StatusProblem.Ok<string>("Validação concluida com sucesso");
        }
    }
}
