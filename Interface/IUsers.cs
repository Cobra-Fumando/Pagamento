using Pic.Parametros;

namespace Pic.Interface
{
    public interface IUsers
    {
        Task<TabelaProblem<UsuarioDto>> Criar(UsuarioDto usuario);
        Task<TabelaProblem<string>> Logar(Logar logar);
    }
}
