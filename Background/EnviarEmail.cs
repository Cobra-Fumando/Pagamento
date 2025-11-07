using Microsoft.EntityFrameworkCore.Metadata;
using Pic.Classes;
using Pic.Interface;
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
        private IConnection? connection;
        private IChannel? channel;
        private readonly object lockObject = new();
        private readonly List<Task> processingTasks = new();
        private string consumeTag = string.Empty;
        private IEmailSmtp emailSmtp;
        public EnviarEmail(ILogger<EnviarEmail> logger, IEmailSmtp emailSmtp)
        {
            this.logger = logger;
            this.emailSmtp = emailSmtp;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            connection = await factory.CreateConnectionAsync();
            channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(FilaA, true, false, false, null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                Task tasks;

                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);

                    var message = JsonSerializer.Deserialize<EmailSerializer>(json);
                    if (message == null) return;

                    logger.LogInformation($" [x] Enviando email para {message.Email}");

                    tasks = emailSmtp.Enviar("Remetente", message.Email, "sdfac@", "Nome", message.Assunto, message.Corpo, message.Token);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro ao processar mensagem da fila");
                    return;
                }

                bool lockTaken = false;

                try
                {
                    Monitor.Enter(lockObject, ref lockTaken);
                    processingTasks.Add(tasks);
                }finally
                {
                    if (lockTaken) Monitor.Exit(lockObject);
                }

                try
                {
                    await tasks;
                }
                finally
                {
                    lockTaken = false;
                    try 
                    {
                        Monitor.Enter(lockObject, ref lockTaken);
                        processingTasks.Remove(tasks);
                    }
                    finally
                    {
                        if(lockTaken) Monitor.Exit(lockObject);
                    }
                }

            };

            consumeTag = await channel.BasicConsumeAsync(FilaA, true, consumer);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            if (channel != null && channel.IsOpen && !string.IsNullOrEmpty(consumeTag))
            {
                await channel.BasicCancelAsync(consumeTag);
            }

            List<Task> tasks;
            bool lockTaken = false;

            try
            {
                Monitor.Enter(lockObject, ref lockTaken);
                tasks = processingTasks.ToList();
            }
            finally 
            { 
                if (lockTaken) Monitor.Exit(lockObject);
            }

            await Task.WhenAny(Task.WhenAll(tasks), Task.Delay(TimeSpan.FromSeconds(60), stoppingToken));

            if (channel!.IsOpen) await channel.DisposeAsync();
            if (connection!.IsOpen) await connection.DisposeAsync();

            logger.LogInformation("Serviço de envio de email está parando.");
        }
    }
}
