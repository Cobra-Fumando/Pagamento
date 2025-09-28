namespace Pic
{
    public class TabelaProblem<T>
    {
        public bool Sucesso { get; set; }
        public required string Mensagem { get; set; }
        public T? Dados { get; set; }
    }
}
