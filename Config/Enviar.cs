using Microsoft.EntityFrameworkCore;
using Pic.Classes;
using Pic.Context;
using Pic.Tables;
using Pic.Interface;

namespace Pic.Config
{
    public class Enviar : IEnviar
    {
        private readonly AppDbContext context;
        public Enviar(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<TabelaProblem<string>> Transacao(int Id, decimal valor, string EmailT)
        {
            if(!EmailVerify.IsValidEmail(EmailT)) return StatusProblem.Fail<string>("Email invalido");
            if(valor <= 0) return StatusProblem.Fail<string>("Valor invalido");
            var emailD = EmailT.ToLower();

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var usuarios = await context.Usuarios
                                .Where(u => u.Id == Id || u.Email.ToLower() == emailD)
                                .ToListAsync();

                var transferir = usuarios.FirstOrDefault(u => u.Id == Id);
                var receber = usuarios.FirstOrDefault(u => u.Email.ToLower() == emailD);

                if(transferir is null) return StatusProblem.Fail<string>("nenhum usuarios encontrado para transferir");
                if(receber is null) return StatusProblem.Fail<string>("nenhum destinatario encontrado");

                if(transferir.Id == receber.Id) return StatusProblem.Fail<string>("não é possivel transferir para si mesmo");
                if(transferir.Saldo < valor) return StatusProblem.Fail<string>("Saldo insuficiente");

                transferir.Saldo -= valor;
                receber.Saldo += valor;

                var transacao = new Transacao
                {
                    UsuarioName = transferir.Nome,
                    UsuarioId = transferir.Id,
                    Valor = valor,
                };

                await context.Transacaos.AddAsync(transacao);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return StatusProblem.Ok<string>("Transação realizada com sucesso");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusProblem.Fail<string>(ex.Message);
            }
        }

        public async Task<TabelaProblem<List<Transacao>>> GetTransacao(int id, int Tamanho, int pagina)
        {
            if (pagina < 1) pagina = 1;
            if (Tamanho < 1) Tamanho = 10;

            try
            {
                var transacoes = await context.Transacaos.AsNoTracking()
                                        .Where(p => p.Id == id)
                                        .Skip((pagina - 1) * Tamanho)
                                        .Take(Tamanho)
                                        .ToListAsync();

                if (transacoes is null || transacoes.Count == 0) return StatusProblem.Fail<List<Transacao>>("Nenhuma transação encontrada");

                return StatusProblem.Ok("Transações encontradas", transacoes);
            }
            catch (Exception ex)
            {
                return StatusProblem.Fail<List<Transacao>>(ex.Message);
            }
        }
    }
}
