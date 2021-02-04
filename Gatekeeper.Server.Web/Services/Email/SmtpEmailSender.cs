using Gatekeeper.Server.Web.Services.Email.Credentials;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace AuthServer.Server.Services.Email
{
    class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpCredentialManager _smtpCredentialManager;

        public SmtpEmailSender(SmtpCredentialManager smtpCredentialManager)
        {
            _smtpCredentialManager = smtpCredentialManager;
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
            SmtpCredentials? credentials = await _smtpCredentialManager.FetchCredentialsAsync();
            if(credentials == null) {
                throw new Exception("SMTP credentials are not configured");
            }

            MimeMessage mail = new MimeMessage();
            mail.From.Add(new MailboxAddress("No Reply", credentials.SenderAddress));
            mail.To.Add(new MailboxAddress(recipientName, email));
            mail.Subject = subject;

            BodyBuilder bodyBuilder = new BodyBuilder ();
            bodyBuilder.TextBody = message;
            mail.Body = bodyBuilder.ToMessageBody();

            using (SmtpClient client = new SmtpClient())
            {
                await client.ConnectAsync(
                    credentials.Hostname, 
                    credentials.Port
                );

                await client.AuthenticateAsync(credentials.Username, credentials.Password);
                await client.SendAsync(mail);
                await client.DisconnectAsync(true);
            }
        }
    }
}
