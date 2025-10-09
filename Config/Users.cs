using Microsoft.EntityFrameworkCore;
using Pic.Classes;
using Pic.Condicao;
using Pic.Context;
using Pic.Parametros;
using Pic.Tables;

namespace Pic.Config
{
    public class Users
    {
        private readonly AppDbContext context;
        private readonly Token token;
        private readonly PasswordHash passwordHash;
        public Users(AppDbContext context, Token token, PasswordHash passwordHash)
        {
            this.context = context;
            this.token = token;
            this.passwordHash = passwordHash;
        }

        public async Task<TabelaProblem<UsuarioDto>> Criar(UsuarioDto usuario)
        {
            if (usuario is null) return StatusProblem.Fail<UsuarioDto>("Dados inválidos");

            var result = CriarUser.ValidarCodicao(usuario);
            if (!result.Sucesso) return StatusProblem.Fail<UsuarioDto>(result.Mensagem);

            bool valido = VerificarCpf.FormatoCpf(usuario.Cpf, out string CpfReplace);
            if (!valido) return StatusProblem.Fail<UsuarioDto>("Formato do cpf invalido");

            try
            {
                var Usuarioexiste = await context.Usuarios
                                    .AsNoTracking().
                                    FirstAsync(p => p.Email.ToLower() == usuario.Email.ToLower() || p.Cpf == CpfReplace);

                if (Usuarioexiste != null)
                {
                    if (usuario.Email.ToLower() == Usuarioexiste.Email.ToLower()) return StatusProblem.Fail<UsuarioDto>("Email já cadastrado");
                    if (Usuarioexiste.Cpf == CpfReplace) return StatusProblem.Fail<UsuarioDto>("Cpf já cadastrado");
                }

                string Hash = passwordHash.Hashar(usuario.Senha);

                var users = new Usuario
                {
                    Nome = usuario.Nome,
                    Senha = Hash,
                    Email = usuario.Email.ToLower(),
                    Cpf = CpfReplace
                };

                usuario.Email = users.Email;

                await context.Usuarios.AddAsync(users);
                await context.SaveChangesAsync();

                return StatusProblem.Ok("Criado com sucesso", usuario);
            }
            catch (DbUpdateException ex) when(
                ex.InnerException?.Message.Contains("UQ_Email") == true || ex.InnerException?.Message.Contains("UQ_Cpf") == true) 
            {
                return StatusProblem.Fail<UsuarioDto>("Email ou Cpf já cadastrado");
            }
            catch (Exception ex)
            {
                return StatusProblem.Fail<UsuarioDto>(ex.Message);
            }
        }

        public async Task<TabelaProblem<string>> Logar(Logar logar)
        {
            try
            {
                var Verificado = EmailVerify.IsValidEmail(logar.Email);
                if (!Verificado) return StatusProblem.Fail<string>("Email inválido");

                var user = await context.Usuarios.AsNoTracking()
                                .Where(p => p.Email == logar.Email)
                                .Select(p => new UsuarioLoginDto
                                {
                                    Id = p.Id,
                                    Nome = p.Nome,
                                    Email = p.Email,
                                    Senha = p.Senha
                                })
                                .FirstOrDefaultAsync();

                if (user is null) return StatusProblem.Fail<string>("Email ou Senha errado");
                if (!passwordHash.Verificar(logar.Senha, user.Senha)) return StatusProblem.Fail<string>("Email ou Senha errado");

                var tokenGerado = token.GenerateToken(user);
                return StatusProblem.Ok("Login realizado com sucesso", tokenGerado);
            }
            catch (Exception ex)
            {
                return StatusProblem.Fail<string>(ex.Message);
            }
        }
    }
}
