using Microsoft.EntityFrameworkCore;
using Pic.Classes;
using Pic.Condicao;
using Pic.Context;
using Pic.Parametros;
using Pic.Tables;
using Pic.Interface;
using Microsoft.Extensions.Caching.Memory;
using Pic.Mensageiro;

namespace Pic.Config
{
    public class Users : IUsers
    {
        private readonly AppDbContext context;
        private readonly Token token;
        private readonly PasswordHash passwordHash;
        private readonly IMemoryCache memoryCache;
        private readonly EnviarRabbit rabbit;
        private readonly ILogger<Users> logger;
        public Users(AppDbContext context, Token token, PasswordHash passwordHash, IMemoryCache memoryCache, EnviarRabbit rabbit, ILogger<Users> logger)
        {
            this.context = context;
            this.token = token;
            this.passwordHash = passwordHash;
            this.memoryCache = memoryCache;
            this.rabbit = rabbit;
            this.logger = logger;
        }

        public async Task<TabelaProblem<string>> Criar(UsuarioDto usuario)
        {
            if (usuario is null) return StatusProblem.Fail<string>("Dados inválidos");

            var result = CriarUser.ValidarCodicao(usuario);
            if (!result.Sucesso) return StatusProblem.Fail<string>(result.Mensagem);

            bool valido = VerificarRegex.FormatoCpf(usuario.Cpf, out string CpfReplace);
            if (!valido) return StatusProblem.Fail<string>("Formato do cpf invalido");

            bool validoTel = VerificarRegex.FormatoTelefone(usuario.Telefone, out string TelefoneReplace);
            if (!validoTel) return StatusProblem.Fail<string>("Formato do telefone invalido");

            try
            {
                var Usuarioexiste = await context.Usuarios
                                    .AsNoTracking().
                                    FirstOrDefaultAsync(p => p.Email.ToLower() == usuario.Email.ToLower() || p.Cpf == CpfReplace || p.Telefone == TelefoneReplace);

                if (Usuarioexiste != null)
                {
                    if (usuario.Email.ToLower() == Usuarioexiste.Email.ToLower()) return StatusProblem.Fail<string>("Email já cadastrado");
                    if (Usuarioexiste.Cpf == CpfReplace) return StatusProblem.Fail<string>("Cpf já cadastrado");
                    if (Usuarioexiste.Telefone == TelefoneReplace) return StatusProblem.Fail<string>("Telefone já cadastrado");
                }

                string Hash = passwordHash.Hashar(usuario.Senha);

                var users = new Usuario
                {
                    Nome = usuario.Nome,
                    Senha = Hash,
                    Email = usuario.Email.ToLower(),
                    Telefone = TelefoneReplace,
                    Cpf = CpfReplace
                };

                string Key = $"{usuario.Nome}_{usuario.Email}_{Guid.NewGuid().ToString()}";
                string Token = token.GenerateTokenConfirm(Key);

                memoryCache.Set(Key, users, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
                });

                usuario.Email = users.Email;

                await rabbit.Enviar(usuario.Email, Token);

                return StatusProblem.Ok("Mensagem enviada para ", usuario.Email);
            }
            catch (DbUpdateException ex) when(
                ex.InnerException?.Message.Contains("UQ_Email") == true || ex.InnerException?.Message.Contains("UQ_Cpf") == true) 
            {
                return StatusProblem.Fail<string>("Email ou Cpf já cadastrado");
            }
            catch (Exception ex)
            {
                return StatusProblem.Fail<string>(ex.Message);
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

        public async Task<TabelaProblem<string>> Confirm(string Token)
        {
            var principal = token.ValidateToken(Token);

            if(principal == null) return StatusProblem.Fail<string>("Token invalido ou expirado");
            var Key = principal.FindFirst("Cache")?.Value;

            if(string.IsNullOrWhiteSpace(Key)) return StatusProblem.Fail<string>("Nenhuma Key encontrada no token");

            if (!memoryCache.TryGetValue(Key, out Usuario? Valor))
            {
                return StatusProblem.Fail<string>("Nada encontrado nessa key");
            }

            if(Valor == null) return StatusProblem.Fail<string>("Nenhum valor encontrado");

            await context.Usuarios.AddAsync(Valor);
            await context.SaveChangesAsync();

            memoryCache.Remove(Key);

            return StatusProblem.Ok<string>("Conta criada com sucesso");
        }
    }
}
