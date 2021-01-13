using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace AuthServer.Server.Services.Email
{
    class SmtpEmailSender : IEmailSender
    {
        private readonly ConfigurationProvider _configuration;

        public SmtpEmailSender(ConfigurationProvider configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            string email, 
            string recipientName, 
            string subject, 
            string message)
        {
             await Execute(email, recipientName, subject, message);
        }

        public async Task Execute(
            string email, 
            string recipientName, 
            string subject, 
            string message)
        {
            string? senderAddress;
            _configuration.TryGet("smtp.senderAddress", out senderAddress);

            MimeMessage mail = new MimeMessage();
            mail.From.Add(new MailboxAddress("No Reply", senderAddress));
            mail.To.Add(new MailboxAddress(recipientName, email));
            mail.Subject = subject;

            BodyBuilder bodyBuilder = new BodyBuilder ();
            bodyBuilder.TextBody = message;
            mail.Body = bodyBuilder.ToMessageBody();

            using (SmtpClient client = new SmtpClient())
            {
                string? smtpHost;
                _configuration.TryGet("smtp.hostname", out smtpHost);

                string? smtpPort;
                _configuration.TryGet("smtp.port", out smtpPort);

                await client.ConnectAsync(
                    smtpHost, 
                    Int32.Parse(smtpPort)
                );

                string? username;
                _configuration.TryGet("smtp.username", out username);
                string? password;
                _configuration.TryGet("smtp.password", out password);
                await client.AuthenticateAsync(username, password);

                await client.SendAsync(mail);
                await client.DisconnectAsync(true);
            }
        }
    }
}
