using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pic.Tables
{
    [Table("Usuario")]
    [Index(nameof(Cpf), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Telefone), IsUnique = true)]
    [Index(nameof(Id), IsUnique = true)]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome não definido")]
        [MaxLength(100)]
        public required string Nome { get; set; }

        [Required(ErrorMessage = "Cpf é obrigatório")]
        [MaxLength(11)]
        public required string Cpf { get; set; }

        [Required(ErrorMessage = "Senha não definida")]
        public required string Senha { get; set; }

        [Required(ErrorMessage = "Email não definido")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Número de telefone é obrigatório")]
        [MaxLength(14)]
        public required string Telefone { get; set; }

        public decimal? Saldo { get; set; } = 0;

        public List<Transacao> transacaos { get; set; } = new();
    }
}
