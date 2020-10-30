using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace AuthServer.Server.Services.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailSender(IConfiguration configuration)
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
            MimeMessage mail = new MimeMessage();
            mail.From.Add(new MailboxAddress("No Reply", "no-reply@example.com"));
            mail.To.Add(new MailboxAddress(recipientName, email));
            mail.Subject = subject;

            BodyBuilder bodyBuilder = new BodyBuilder ();
            bodyBuilder.TextBody = message;
            mail.Body = bodyBuilder.ToMessageBody();

            using (SmtpClient client = new SmtpClient())
            {
                await client.ConnectAsync(
                    _configuration["SMTP_HOST"], 
                    Int32.Parse(_configuration["SMTP_PORT"])
                );
                await client.SendAsync(mail);
                await client.DisconnectAsync(true);
            }
        }
    }
}
