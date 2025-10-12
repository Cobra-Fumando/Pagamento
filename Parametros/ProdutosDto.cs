namespace Pic.Parametros
{
    public class ProdutosDto
    {
        public required string Nome { get; set; }
        public required decimal Preco { get; set; }
        public string? Descricao { get; set; }
        public required int Estoque { get; set; }
        public required string Email { get; set; }
    }
}
