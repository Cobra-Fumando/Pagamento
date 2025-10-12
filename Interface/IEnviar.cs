using Pic.Tables;

namespace Pic.Interface
{
    public interface IEnviar
    {
        Task<TabelaProblem<string>> Transacao(int Id, decimal valor, string EmailT);
        Task<TabelaProblem<List<Transacao>>> GetTransacao(int id, int Tamanho, int Pagina);
    }
}
