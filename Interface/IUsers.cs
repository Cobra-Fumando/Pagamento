using Pic.Tables.Models;

namespace Pic.Interface
{
    public interface IUsers
    {
        Task<TabelaProblem<string>> Criar(UsuarioDto usuario);
        Task<TabelaProblem<string>> Logar(Logar logar);
        Task<TabelaProblem<string>> Confirm(string Token);
    }
}
