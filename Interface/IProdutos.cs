using Pic.Parametros;
using Pic.Tables;

namespace Pic.Interface
{
    public interface IProdutos
    {
        Task<TabelaProblem<Produto>> AdicionarProduto(ProdutosDto produto, int id);
        Task<TabelaProblem<List<Produto>>> ListarProdutos(int Tamanho, int Pagina);
        Task<TabelaProblem<List<Produto>>> SeusProdutos(int id, int Tamanho, int Pagina);
    }
}
