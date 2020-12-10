using System.Threading.Tasks;

namespace AuthServer.Server.Services.TLS
{
    interface IRequestAcmeCertificateJob {
        Task Request(string contactEmail, string domainName);
    }
}
