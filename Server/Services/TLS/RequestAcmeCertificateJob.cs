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
        private readonly ConfigurationProvider _configurationProvider;
        const string ACCOUNT_KEYNAME = "tls.acme.accountKey";

        public RequestAcmeCertificateJob(
            AcmeChallengeSingleton challengeSingleton,
            ConfigurationProvider configurationProvider)
        {
            _challengeSingleton = challengeSingleton;
            _configurationProvider = configurationProvider;
        }

        public async Task Request(string contactEmail, string domainName)
        {
            bool hasAcmeAccount = _configurationProvider.TryGet(ACCOUNT_KEYNAME, out string accountKeyString);

            AcmeContext acmeContext;
            IAccountContext accountContext;
            if (hasAcmeAccount)
            {
                IKey accountKey = KeyFactory.FromPem(accountKeyString);
                acmeContext = new AcmeContext(WellKnownServers.LetsEncryptV2, accountKey);
            }
            else
            {
                acmeContext = new AcmeContext(WellKnownServers.LetsEncryptV2);
                accountContext = await acmeContext.NewAccount(contactEmail, true);
                _configurationProvider.Set(ACCOUNT_KEYNAME, acmeContext.AccountKey.ToPem());
            }

            var order = await acmeContext.NewOrder(new[] { domainName });

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
