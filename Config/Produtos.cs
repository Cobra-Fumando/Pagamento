using Pic.Classes;
using Pic.Context;
using Pic.Interface;
using Pic.Parametros;
using Pic.Tables;
using Microsoft.EntityFrameworkCore;

namespace Pic.Config
{
    public class Produtos : IProdutos
    {
        private readonly AppDbContext context;
        private readonly IEnviar enviar;
        public Produtos(AppDbContext context, IEnviar enviar) 
        {
            this.context = context;
            this.enviar = enviar;
        }

        public async Task<TabelaProblem<Produto>> AdicionarProduto(ProdutosDto produto, int id)
        {
            if (produto is null) return StatusProblem.Fail<Produto>("Produto está vazio");

            try
            {
                var user = await context.Usuarios
                                .AsNoTracking()
                                .FirstOrDefaultAsync(p => p.Id == id);

                if(user is null) return StatusProblem.Fail<Produto>("Usuario não encontrado");

                var product = new Produto
                {
                    Nome = produto.Nome,
                    UsuarioId = user.Id,
                    Email = user.Email,
                    Descricao = produto.Descricao,
                    Preco = produto.Preco,
                    Estoque = produto.Estoque
                };

                await context.Produto.AddAsync(product);
                await context.SaveChangesAsync();
                return StatusProblem.Ok("Produto adicionado com sucesso", product);
            }
            catch(Exception ex)
            {
                return StatusProblem.Fail<Produto>(ex.Message);
            }
        }

        public async Task<TabelaProblem<List<Produto>>> ListarProdutos(int Tamanho, int Pagina)
        {
            if(Pagina < 1) Pagina = 1;
            if(Tamanho < 1) Tamanho = 10;

            try
            {
                var produtos = await context.Produto
                                    .AsNoTracking()
                                    .Skip((Pagina - 1) * Tamanho)
                                    .Take(Tamanho)
                                    .ToListAsync();

                if (produtos is null || produtos.Count == 0) return StatusProblem.Fail<List<Produto>>("Nenhum produto encontrado");
                return StatusProblem.Ok("Produtos encontrados com sucesso", produtos);
            }
            catch(Exception ex)
            {
                return StatusProblem.Fail<List<Produto>>(ex.Message);
            }
        }

        public async Task<TabelaProblem<List<Produto>>> SeusProdutos(int id, int Tamanho, int Pagina)
        {
            if (Pagina < 1) Pagina = 1;
            if (Tamanho < 1) Tamanho = 10;

            try
            {

                var produtos = await context.Produto
                                    .AsNoTracking()
                                    .Where(p => p.UsuarioId == id)
                                    .Skip((Pagina - 1) * Tamanho)
                                    .Take(Tamanho)
                                    .ToListAsync();

                if (produtos is null || produtos.Count == 0) return StatusProblem.Fail<List<Produto>>("Nenhum produto encontrado");
                return StatusProblem.Ok("Produtos encontrados com sucesso", produtos);
            }
            catch (Exception ex)
            {
                return StatusProblem.Fail<List<Produto>>(ex.Message);
            }
        }

        public async Task<TabelaProblem<Produto>> PagarProduto(Produto produto, int id)
        {
            try
            {
                var User = await context.Usuarios
                                .AsNoTracking()
                                .FirstOrDefaultAsync(p => p.Id == id);

                if (User is null) return StatusProblem.Fail<Produto>("Usuario não encontrado");

                var prod = await context.Produto
                                .AsNoTracking()
                                .FirstOrDefaultAsync(p => p.Id == produto.Id);

                if (prod is null) return StatusProblem.Fail<Produto>("Produto não encontrado");

                var result = await enviar.Transacao(User.Id, prod.Preco, produto.Email);
                if (!result.Sucesso) return StatusProblem.Fail<Produto>(result.Mensagem);

                return StatusProblem.Ok<Produto>("Produto comprado com sucesso");
            }
            catch (Exception ex)
            {
                return StatusProblem.Fail<Produto>(ex.Message);
            }
        }
    }
}
