using System;
using System.IO;
using System.Threading.Tasks;
using Certes;
using Certes.Acme;
using Certes.Acme.Resource;

namespace AuthServer.Server.Services.TLS
{
    class RequestAcmeCertificateJob : IRequestAcmeCertificateJob
    {
        private readonly AcmeChallengeSingleton _challengeSingleton;

        public RequestAcmeCertificateJob(AcmeChallengeSingleton challengeSingleton)
        {
            _challengeSingleton = challengeSingleton;
        }

        public async Task Request(string contactEmail, string domainName)
        {
            var acme = new AcmeContext(WellKnownServers.LetsEncryptStagingV2);
            var account = await acme.NewAccount(contactEmail, true);

            var order = await acme.NewOrder(new[] { domainName });

            var authorizations = await order.Authorization(domainName);
            var httpChallenge = await authorizations.Http();

            _challengeSingleton.AddChallenge(httpChallenge.Token, httpChallenge.KeyAuthz);

            Challenge challenge;

            int tryCount = 1;
            do
            {
                await Task.Delay(5000 * tryCount);
                challenge = await httpChallenge.Validate();
            }
            while (challenge.Status == ChallengeStatus.Pending && ++tryCount <= 20);

            if (challenge.Status != ChallengeStatus.Valid)
            {
                throw new Exception("Challenge status is invalid.");
            }

            var certKey = KeyFactory.NewKey(KeyAlgorithm.RS256);
            await order.Finalize(new CsrInfo { }, certKey);
            var certChain = await order.Download();

            var pfxBuilder = certChain.ToPfx(certKey);
            
            string targetLocation = CertificateLocationHelper.GetPath(domainName);
            File.WriteAllBytes(targetLocation, pfxBuilder.Build(domainName, ""));
        }
    }
}
