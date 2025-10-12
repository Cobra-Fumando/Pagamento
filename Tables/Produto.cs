using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pic.Tables
{
    [Table("Produtos")]
    [Index(nameof(Email), IsUnique = false)]
    public class Produto
    {
        [Key]
        public int Id { get; set; }
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "Nome não pode ser vazio")]
        [MaxLength(100)]
        public required string Nome { get; set; }

        [Required(ErrorMessage = "Preço não pode estar vazio")]
        public required decimal Preco { get; set; }
        public string? Descricao { get; set; }
        public required int Estoque { get; set; }

        [Required(ErrorMessage = "Email não pode estar vazio")]
        public required string Email { get; set; }
        //public string ImagemUrl { get; set; }
    }
}
