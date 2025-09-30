using Pic.Tables;

namespace Pic.Parametros
{
    public class UsuarioLoginDto
    {
        public required int Id { get; set; }
        public required string Nome { get; set; }
        public required string Email { get; set; }
        public required string Senha { get; set; }
    }
}
