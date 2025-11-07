using Pic.Tables;
using Pic.Tables.Models;

namespace Pic.Interface
{
    public interface IProdutos
    {
        Task<TabelaProblem<Produto>> AdicionarProduto(ProdutosDto produto, int id);
        Task<TabelaProblem<List<Produto>>> ListarProdutos(int Tamanho, int Pagina);
        Task<TabelaProblem<List<Produto>>> SeusProdutos(int id, int Tamanho, int Pagina);
        Task<TabelaProblem<Produto>> PagarProduto(Produto produto, int id, string Token);
    }
}
