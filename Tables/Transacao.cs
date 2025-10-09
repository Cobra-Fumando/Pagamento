using System.ComponentModel.DataAnnotations;

namespace Pic.Tables
{
    public class Transacao
    {
        [Key]
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        [Required]
        public string UsuarioName { get; set; } = string.Empty;
        [Required]
        public decimal Valor { get; set; }
        public DateTime Data { get; } = DateTime.UtcNow;
    }
}
