using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pic.Tables
{
    [Table("Usuario")]
    [Index(nameof(Cpf), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Id), IsUnique = true)]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome não definido")]
        public required string Nome { get; set; }

        [Required(ErrorMessage = "Cpf é obrigatório")]
        public required string Cpf { get; set; }

        [Required(ErrorMessage = "Senha não definida")]
        public required string Senha { get; set; }

        [Required(ErrorMessage = "Email não definido")]
        public required string Email { get; set; }

        public decimal? Saldo { get; set; } = 0;

        public List<Transacao> transacaos { get; set; } = new();
    }
}
