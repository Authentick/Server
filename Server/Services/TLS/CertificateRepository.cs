using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gatekeeper.Server.Services.FileStorage;
using NodaTime;

namespace AuthServer.Server.Services.TLS
{
    public class CertificateRepository
    {
        public List<Certificate> GetAll()
        {
            string snapFolder = PathProvider.GetApplicationDataFolder();

            DirectoryInfo info = new DirectoryInfo(snapFolder);
            FileInfo[] files = info
                .GetFiles()
                .Where(f => f.Extension == ".pfx")
                .OrderBy(f => f.CreationTime)
                .ToArray();

            List<Certificate> certificateList = new List<Certificate>();

            foreach (FileInfo file in files)
            {
                Certificate cert = new Certificate(
                    file.Name.Substring(0, file.Name.Length - 4),
                    Instant.FromDateTimeUtc(file.LastWriteTimeUtc)
                );
                certificateList.Add(cert);
            }

            return certificateList;
        }

        public class Certificate
        {

            public readonly string Domain;
            public readonly Instant LastIssuedTime;

            public Certificate(string domain, Instant lastIssuedTime)
            {
                Domain = domain;
                LastIssuedTime = lastIssuedTime;
            }
        }
    }
}
