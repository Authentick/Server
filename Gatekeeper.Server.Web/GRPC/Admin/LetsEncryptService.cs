using System.Collections.Generic;
using System.Threading.Tasks;
using AuthServer.Server.Services.TLS;
using AuthServer.Shared.Admin;
using Gatekeeper.Server.Services.FileStorage;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace AuthServer.Server.GRPC.Admin
{
    [Authorize(Policy = "SuperAdministrator")]
    public class LetsEncryptService : AuthServer.Shared.Admin.LetsEncrypt.LetsEncryptBase
    {
        private readonly CertificateRepository _certificateRespository;

        public LetsEncryptService(CertificateRepository certificateRepository)
        {
            _certificateRespository = certificateRepository;
        }

        public override Task<CertificateListReply> ListCertificates(Empty request, ServerCallContext context)
        {
            string path = PathProvider.GetApplicationDataFolder();

            CertificateListReply reply = new CertificateListReply();
            List<CertificateRepository.Certificate> certificates = _certificateRespository.GetAll();

            foreach (CertificateRepository.Certificate certificate in certificates)
            {
                Certificate cert = new Certificate
                {
                    Domain = certificate.Domain,
                    LastIssued = NodaTime.Serialization.Protobuf.NodaExtensions.ToTimestamp(certificate.LastIssuedTime),
                };
                reply.Certificates.Add(cert);
            }

            return Task.FromResult(reply);
        }
    }
}
