using Microsoft.EntityFrameworkCore;
using Pic.Classes;
using Pic.Context;

namespace Pic.Config
{
    public class Enviar
    {
        private readonly AppDbContext context;
        public Enviar(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<TabelaProblem<string>> Transacao(int Id, decimal valor, string EmailT)
        {
            if(!EmailVerify.IsValidEmail(EmailT)) return StatusProblem.Fail("Email invalido");
            if(valor <= 0) return StatusProblem.Fail("Valor invalido");
            var emailD = EmailT.ToLower();

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var usuarios = await context.Usuarios
                                .Where(u => u.Id == Id || u.Email.ToLower() == emailD)
                                .ToListAsync();

                var transferir = usuarios.FirstOrDefault(u => u.Id == Id);
                var receber = usuarios.FirstOrDefault(u => u.Email.ToLower() == emailD);

                if(transferir is null) return StatusProblem.Fail("nenhum usuarios encontrado para transferir");
                if(receber is null) return StatusProblem.Fail("nenhum destinatario nao encontrado");

                if(transferir.Id == receber.Id) return StatusProblem.Fail("não é possivel transferir para si mesmo");
                if(transferir.Saldo < valor) return StatusProblem.Fail("Saldo insuficiente");

                transferir.Saldo -= valor;
                receber.Saldo += valor;

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return StatusProblem.Ok("Transação realizada com sucesso");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusProblem.Fail(ex.Message);
            }
        }
    }
}
