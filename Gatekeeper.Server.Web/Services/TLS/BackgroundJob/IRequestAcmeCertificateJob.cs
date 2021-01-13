using System.Threading.Tasks;

namespace AuthServer.Server.Services.TLS.BackgroundJob
{
    interface IRequestAcmeCertificateJob {
        Task Request(string contactEmail, string domainName);
    }
}
