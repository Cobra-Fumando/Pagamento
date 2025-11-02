namespace Pic.Interface
{
    public interface IEnviaRabbit
    {
        Task Enviar(string Email, string Token);
    }
}
