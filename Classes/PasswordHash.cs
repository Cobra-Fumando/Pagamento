using Microsoft.AspNetCore.Identity;
using Pic.Config;

namespace Pic.Classes
{
    public class PasswordHash
    {
        private readonly IPasswordHasher<Users> _passwordHasher;
        public PasswordHash(IPasswordHasher<Users> passwordHasher)
        {
            this._passwordHasher = passwordHasher;
        }

        public string Hashar(string Senha)
        {
            return _passwordHasher.HashPassword(null, Senha);
        }

        public bool Verificar(string Senha, string Hash)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, Hash, Senha);
            return result == PasswordVerificationResult.Success;
        }
    }
}
