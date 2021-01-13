using System;

namespace Gatekeeper.Server.Services.FileStorage
{
    static class PathProvider
    {
        public static string GetApplicationDataFolder()
        {
            string? snapFolder = Environment.GetEnvironmentVariable("SNAP_USER_DATA");
            if (snapFolder == null)
            {
                // FIXME: This is for development installations and not really great. This should not be the temp folder but 
                // something more dedicated.
                snapFolder = "/tmp";
            }

            return snapFolder;
        }
    }
}
