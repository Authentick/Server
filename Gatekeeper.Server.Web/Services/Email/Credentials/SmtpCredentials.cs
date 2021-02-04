namespace Gatekeeper.Server.Web.Services.Email.Credentials 
{
    public class SmtpCredentials
    {
        public string? Hostname { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? SenderAddress { get; set; } 
        public int Port { get; set; }
    }
}
