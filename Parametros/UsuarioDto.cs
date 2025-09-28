using System.ComponentModel.DataAnnotations;

namespace Pic.Parametros
{
    public class UsuarioDto
    {
        public required string Nome { get; set; }
        public required string Senha { get; set; }
        public required string Email { get; set; }
    }
}
