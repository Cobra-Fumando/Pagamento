using Pic.Classes;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Pic.Background
{
    public class EnviarEmail : BackgroundService
    {
        private string FilaA = "FilaA";
        private readonly ILogger<EnviarEmail> logger;
        public EnviarEmail(ILogger<EnviarEmail> logger)
        {
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(FilaA, true, false, false, null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                var message = JsonSerializer.Deserialize<EmailSerializer>(json);
                if(message == null) return;

                logger.LogInformation($" [x] Enviando email para {message.Email}");

                //Enviar Email Depois faço
            };

            await channel.BasicConsumeAsync(FilaA, true, consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
