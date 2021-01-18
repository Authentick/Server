using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Gatekeeper.LdapServerLibrary;
using Gatekeeper.Server.Ldap.EventHandler;
using Grpc.Net.Client;

namespace Gatekeeper.Server.Ldap
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // FIXME: This is only okay as we are connecting against localhost.
            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            using var channel = GrpcChannel.ForAddress(
                "https://localhost",
                new GrpcChannelOptions
                {
                    HttpHandler = httpHandler
                }
            );
            var client = new Shared.LdapAndWeb.Ldap.LdapClient(channel);

            string? certificatePath = null; 
            while(certificatePath == null)
            {
                try {
                    certificatePath = (await client.GetCertificatePathAsync(new Google.Protobuf.WellKnownTypes.Empty { })).Path;
                } catch {
                    System.Console.WriteLine("Could not fetch certificate from remote host. Retrying in 3 seconds.");
                    await Task.Delay(3 * 1000);
                }
            }

            LdapServer server = new LdapServer
            {
                Port = 389,
                IPAddress = IPAddress.Parse("0.0.0.0"),
            };
            server.RegisterLogger(new ConsoleLogger());
            server.RegisterEventListener(new LdapEventListener(client));
            server.RegisterCertificate(new System.Security.Cryptography.X509Certificates.X509Certificate2(certificatePath));
            await server.Start();
        }
    }
}
