using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Gatekeeper.Server.Services.FileStorage
{
    public class ProfileImageManager
    {
        private string GetImagePath(Guid userId)
        {
            string directory = PathProvider.GetApplicationDataFolder() + "/profile-images/";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return directory + userId + ".jpg";
        }

        public bool HasProfileImage(Guid userId)
        {
            return File.Exists(GetImagePath(userId));
        }

        public async Task StoreImageAsync(Guid userId, IFormFile formFile)
        {
            FileStream fileStream = GetImageStream(userId, FileMode.OpenOrCreate);
            await formFile.CopyToAsync(fileStream);
        }

        public FileStream GetImageStream(Guid userId, FileMode mode)
        {
            return new FileStream(GetImagePath(userId), mode);
        }
    }
}
