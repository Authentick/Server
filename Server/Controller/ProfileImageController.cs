using System;
using System.Threading.Tasks;
using AuthServer.Server.Services.User;
using Gatekeeper.Server.Services.FileStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gatekeeper.Server.Controller
{
    [Route("api/profile/image/")]
    [Authorize]
    public class ProfileImageController : ControllerBase
    {
        private readonly ProfileImageManager _profileImageManager;
        private readonly UserManager _userManager;

        public ProfileImageController(
            ProfileImageManager profileImageManager,
            UserManager userManager)
        {
            _profileImageManager = profileImageManager;
            _userManager = userManager;
        }

        [HttpPut]
        public async Task<IActionResult> UploadProfileImage()
        {
            Guid userId = new Guid(_userManager.GetUserId(HttpContext.User));

            if (HttpContext.Request.Form.Files.Count == 1)
            {
                IFormFile uploadedFile = HttpContext.Request.Form.Files[0];
                await _profileImageManager.StoreImageAsync(userId, uploadedFile);
                return Ok();
            }

            return BadRequest();
        }

        [HttpGet("{userId}")]
        public IActionResult GetProfileImage(Guid userId)
        {
            if (_profileImageManager.HasProfileImage(userId))
            {
                return File(_profileImageManager.GetImageStream(userId, System.IO.FileMode.Open), "image/jpeg");
            }

            return NotFound();
        }
    }
}
