using System.IO;
using Gatekeeper.Server.Services.FileStorage;

namespace AuthServer.Server.Services.TLS
{
    class CertificateLocationHelper
    {
        public static string GetPath(string domain)
        {
            string snapFolder = PathProvider.GetApplicationDataFolder();
            string targetHostName = domain.Replace("/", "").Replace("\\", "");
            return snapFolder + "/" + targetHostName + ".pfx";
        }

        public static bool CertificateExists(string domain)
        {
            return File.Exists(GetPath(domain));
        }
    }
}
