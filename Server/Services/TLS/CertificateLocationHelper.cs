using System;
using System.IO;

namespace AuthServer.Server.Services.TLS
{
    class CertificateLocationHelper
    {
        public static string GetPath(string domain)
        {
            string? snapFolder = Environment.GetEnvironmentVariable("SNAP_USER_DATA");

            if (snapFolder == null)
            {
                // FIXME: This is for development installations and not really great. This should not be the temp folder but 
                // something more dedicated.
                snapFolder = "/tmp";
            }

            string targetHostName = domain.Replace("/", "").Replace("\\", "");
            return snapFolder + "/" + targetHostName + ".pfx";
        }

        public static bool CertificateExists(string domain)
        {
            return File.Exists(GetPath(domain));
        }
    }
}
