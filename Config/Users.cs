using Microsoft.EntityFrameworkCore;
using Pic.Classes;
using Pic.Context;
using Pic.Parametros;
using Pic.Tables;

namespace Pic.Config
{
    public class Users
    {
        private readonly AppDbContext context;
        private readonly Token token;
        public Users(AppDbContext context, Token token)
        {
            this.context = context;
            this.token = token;
        }

        public async Task<TabelaProblem<string>> Criar(UsuarioDto usuario)
        {
            var users = new Usuario
            {
                Nome = usuario.Nome,
                Senha = usuario.Senha,
                Email = usuario.Email.ToLower(),
            };

            await context.Usuarios.AddAsync(users);
            await context.SaveChangesAsync();

            return StatusProblem.Ok("Criado com sucesso");
        }

        public async Task<TabelaProblem<string>> Logar(Logar logar)
        {
            var Verificado = EmailVerify.IsValidEmail(logar.Email);
            if (!Verificado) return StatusProblem.Fail("Email inválido");

            var user = await context.Usuarios.Where(p => p.Email == logar.Email && p.Senha == logar.Senha).FirstOrDefaultAsync();

            if(user is null) return StatusProblem.Fail("Usuario ou Senha errado");

            var tokenGerado = token.GenerateToken(user);
            return StatusProblem.Ok("Login realizado com sucesso", tokenGerado);
        }
    }
}
