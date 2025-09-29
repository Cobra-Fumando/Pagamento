namespace Pic.Classes
{
    public class StatusProblem
    {
        public static TabelaProblem<T> Ok<T>(string message = "Sucesso", T? data = default)
        {
            return new TabelaProblem<T>
            {
                Sucesso = true,
                Mensagem = message,
                Dados = data
            };
        }

        public static TabelaProblem<T> Fail<T>(string message = "Ocorreu um erro", T? data = default)
        {
            return new TabelaProblem<T>
            {
                Sucesso = false,
                Mensagem = message,
                Dados = data
            };
        }
    }
}
