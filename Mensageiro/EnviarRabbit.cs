using Pic.Classes;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Pic.Mensageiro
{
    public class EnviarRabbit
    {
        private string FilaA = "FilaA";
        public EnviarRabbit() { }

        public async Task Enviar(string Email, string Token)
        {
            var factory = new ConnectionFactory() { HostName = "localhost"};
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
        }
    }
}
