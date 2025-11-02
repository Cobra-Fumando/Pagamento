using MailKit.Security;
using MimeKit;
using Pic.Interface;
using System;

namespace Pic.Classes
{
    public class EmailSmtp : IEmailSmtp
    {
        private readonly ILogger<EmailSmtp> logger;
        public EmailSmtp(ILogger<EmailSmtp> logger)
        {
            this.logger = logger;
        }
        public async Task Enviar(string RemetenteEmail, string DestinatarioEmail, string Senha, string DestinatarioName, string Mensagem, string assunto, string Token)
        {
            if (string.IsNullOrWhiteSpace(RemetenteEmail) ||
                string.IsNullOrWhiteSpace(DestinatarioEmail) ||
                string.IsNullOrWhiteSpace(Senha) ||
                string.IsNullOrWhiteSpace(DestinatarioName) ||
                string.IsNullOrWhiteSpace(Mensagem) ||
                string.IsNullOrWhiteSpace(Token))
            {
                logger.LogWarning("Está faltando informações");
                return;
            }

            //-----------------------------------------------------------//
            //Mensagem configuração

            var mensagem = new MimeMessage();
            mensagem.From.Add(new MailboxAddress("Ronaldo", RemetenteEmail));
            mensagem.To.Add(new MailboxAddress(DestinatarioName, DestinatarioEmail));

            mensagem.Subject = assunto;
            var url = $"http://localhost:5000/api/Usuarios/Confirm?Token={Token}"; 

            mensagem.Body = new TextPart("html")
            {
                Text = $@"
                        <h1>Olá!</h1>
                        <p>{Mensagem}</p>
                        <p>
                            <a href='{url}' style='
                                display: inline-block;
                                padding: 10px 20px;
                                font-size: 16px;
                                color: white;
                                background-color: #007BFF;
                                text-decoration: none;
                                border-radius: 5px;
                            '>Clique aqui para confirmar</a>
                        </p>"
            };

            //-----------------------------------------------------------//
            //enviar Mensagem

            try
            {
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(RemetenteEmail, Senha);
                await smtp.SendAsync(mensagem);
                await smtp.DisconnectAsync(true);

                logger.LogInformation("Mensagem enviada com sucesso");
            }
            catch (Exception ex)
            {
                logger.LogError($"Erro inesperado: {ex.Message}");
            }
        }

        public async Task<TabelaProblem<string>> CodigoSend(string RemetenteEmail, string DestinatarioEmail, string Senha, string DestinatarioName, string Codigo)
        {

            if (string.IsNullOrWhiteSpace(RemetenteEmail) ||
                string.IsNullOrWhiteSpace(DestinatarioEmail) ||
                string.IsNullOrWhiteSpace(Senha) ||
                string.IsNullOrWhiteSpace(DestinatarioName))
            {
                logger.LogWarning("Está faltando informações");
                return StatusProblem.Fail<string>("Está faltando informações");
            }

            //-----------------------------------------------------------//
            //Mensagem configuração

            var mensagem = new MimeMessage();
            mensagem.From.Add(new MailboxAddress("Ronaldo", RemetenteEmail));
            mensagem.To.Add(new MailboxAddress(DestinatarioName, DestinatarioEmail));

            mensagem.Subject = "Verificação 2 etapa";

            mensagem.Body = new TextPart("html")
            {
                Text = $"<h1>Olá!</h1><p>Este é um e-mail com seu codigo <strong> {Codigo} </strong>.</p>"
            };

            //-----------------------------------------------------------//
            //enviar Mensagem

            try
            {
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(RemetenteEmail, Senha);
                await smtp.SendAsync(mensagem);
                await smtp.DisconnectAsync(true);

                logger.LogInformation("Mensagem enviada com sucesso");

                return StatusProblem.Ok<string>("Mensagem enviada com sucesso");
            }
            catch (Exception ex)
            {
                logger.LogError($"Erro inesperado: {ex.Message}");
                return StatusProblem.Fail<string>($"Erro inesperado: {ex.Message}");
            }
        }
    }
}

