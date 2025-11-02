namespace Pic.Interface
{
    public interface IEmailSmtp
    {
        Task Enviar(string RemetenteEmail, string DestinatarioEmail, string Senha, string DestinatarioName, string Mensagem, string assunto, string Token);
        Task<TabelaProblem<string>> CodigoSend(string RemetenteEmail, string DestinatarioEmail, string Senha, string DestinatarioName, string Codigo);
    }
}
