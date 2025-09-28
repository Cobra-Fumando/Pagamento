namespace Pic.Classes
{
    public class StatusProblem
    {
        public static TabelaProblem<string> Ok(string message = "Sucesso", string data = "")
        {
            return new TabelaProblem<string>
            {
                Sucesso = true,
                Mensagem = message,
                Dados = data
            };
        }

        public static TabelaProblem<string> Fail(string message = "Ocorreu um erro", string data = "")
        {
            return new TabelaProblem<string>
            {
                Sucesso = false,
                Mensagem = message,
                Dados = data
            };
        }
    }
}
