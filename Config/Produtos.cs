using Pic.Classes;
using Pic.Context;
using Pic.Interface;
using Pic.Tables;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Pic.Tables.Models;

namespace Pic.Config
{
    public class Produtos : IProdutos
    {
        private readonly AppDbContext context;
        private readonly IEnviar enviar;
        private readonly HttpClient client;
        private readonly ILogger<Produtos> logger;
        public Produtos(AppDbContext context, IEnviar enviar, IHttpClientFactory httpClientFactory, ILogger<Produtos> logger)
        {
            this.context = context;
            this.enviar = enviar;
            client = httpClientFactory.CreateClient("PicApi");
            this.logger = logger;
        }

        public async Task<TabelaProblem<Produto>> AdicionarProduto(ProdutosDto produto, int id)
        {
            if (produto is null) return StatusProblem.Fail<Produto>("Produto está vazio");

            try
            {
                var user = await context.Usuarios
                                .AsNoTracking()
                                .FirstOrDefaultAsync(p => p.Id == id);

                if (user is null) return StatusProblem.Fail<Produto>("Usuario não encontrado");

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
            catch (DbUpdateException ex)
            {
                logger.LogError("erro ao consultar os dados");
                return StatusProblem.Fail<Produto>($"Erro ao consultar os dados {ex}");
            }
            catch (System.Data.Common.DbException ex)
            {
                logger.LogError("Erro de acesso ao banco de dados");
                return StatusProblem.Fail<Produto>($"Erro de conexão ou comando SQL no banco de dados: {ex}");
            }
            catch (Exception ex)
            {
                return StatusProblem.Fail<Produto>(ex.Message);
            }
        }

        public async Task<TabelaProblem<List<Produto>>> ListarProdutos(int Tamanho, int Pagina)
        {
            if (Pagina < 1) Pagina = 1;
            if (Tamanho < 1) Tamanho = 10;

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
            catch (System.Data.Common.DbException ex)
            {
                logger.LogError("Erro de acesso ao banco de dados");
                return StatusProblem.Fail<List<Produto>>($"Erro de conexão ou comando SQL no banco de dados: {ex}");
            }
            catch (Exception ex)
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
            catch (System.Data.Common.DbException ex)
            {
                logger.LogError("Erro de acesso ao banco de dados");
                return StatusProblem.Fail<List<Produto>>($"Erro de conexão ou comando SQL no banco de dados: {ex}");
            }
            catch (Exception ex)
            {
                return StatusProblem.Fail<List<Produto>>(ex.Message);
            }
        }

        public async Task<TabelaProblem<Produto>> PagarProduto(Produto produto, int id, string Token)
        {
            const string url = "api/enviar";
            int tentativas = 3;

            Usuario? User = null;
            Produto? prod = null;

            for (int i = 0; i < tentativas; i++)
            {
                try
                {
                    if (User is null && prod is null)
                    {
                        try
                        {
                            User = await context.Usuarios
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(p => p.Id == id);

                            if (User is null) return StatusProblem.Fail<Produto>("Usuario não encontrado");

                            prod = await context.Produto
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(p => p.Id == produto.Id);

                            if (prod is null) return StatusProblem.Fail<Produto>("Produto não encontrado");
                        }
                        catch (DbUpdateException ex)
                        {
                            logger.LogError("erro ao consultar os dados");
                            return StatusProblem.Fail<Produto>($"Erro ao consultar os dados {ex}");
                        }
                        catch (System.Data.Common.DbException ex)
                        {
                            logger.LogError("Erro de acesso ao banco de dados");
                            return StatusProblem.Fail<Produto>($"Erro de conexão ou comando SQL no banco de dados: {ex}");
                        }
                        catch (Exception ex)
                        {
                            logger.LogError("Erro inesperado ao consultar os dados");
                            return StatusProblem.Fail<Produto>($"Erro inesperado ao consultar os dados: {ex}");
                        }
                    }

                    if (User?.Id == prod?.UsuarioId) return StatusProblem.Fail<Produto>("Não pode comprar o proprio produto");

                    var transfer = new Transferir
                    {
                        EmailT = prod.Email,
                        Valor = prod.Preco
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(transfer);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

                    var response = await client.PostAsync(url, content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return StatusProblem.Fail<Produto>($"Erro ao processar pagamento {result}");
                    }

                    break;
                }
                catch (Exception ex)
                {
                    if (i == tentativas - 1)
                    {
                        logger.LogError(ex, "Falha ao processar pagamento após várias tentativas.");
                        throw;
                    }

                    logger.LogWarning(ex, $"Tentativa {i + 1} falhou ao processar pagamento. Retentando...");
                    await Task.Delay(2000);
                }
            }
            return StatusProblem.Ok<Produto>("Produto comprado com sucesso");
        }
    }
}
