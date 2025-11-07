using Pic.Classes;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Pic.Interface;

namespace Pic.Mensageiro
{
    public class EnviarRabbit : IEnviaRabbit
    {
        private string FilaA = "FilaA";
        private readonly ILogger<EnviarRabbit> logger;
        public EnviarRabbit(ILogger<EnviarRabbit> logger)
        {
            this.logger = logger;
        }

        public async Task Enviar(string Email, string Token)
        {
            int tentativas = 3;

            for (int i = 0; i < tentativas; i++)
            {
                try
                {

                    var factory = new ConnectionFactory() { HostName = "localhost" };
                    using var connection = await factory.CreateConnectionAsync();
                    using var channel = await connection.CreateChannelAsync();

                    await channel.QueueDeclareAsync(FilaA, true, false, false, null);

                    var email = new EmailSerializer
                    {
                        Email = Email,
                        Assunto = "Confirmação de email",
                        Corpo = "Alguém está logando usando seu email é você?",
                        Token = Token
                    };

                    var json = JsonSerializer.Serialize(email);

                    var Corpo = Encoding.UTF8.GetBytes(json);

                    await channel.BasicPublishAsync("", FilaA, Corpo);

                    logger.LogInformation($"Mensagem enviada para fila com sucesso: {Email}");
                    break;
                }
                catch (Exception ex)
                {
                    if(i == tentativas - 1)
                    {
                        logger.LogError(ex, "Falha ao enviar mensagem para fila após várias tentativas.");
                        throw;
                    }

                    logger.LogWarning(ex, $"Tentativa {i + 1} falhou ao enviar mensagem para fila. Retentando...");
                    await Task.Delay(2000);
                }
            }
        }
    }
}
