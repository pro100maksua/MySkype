using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySkype.Server.Logic.Interfaces;

namespace MySkype.Server.Controllers
{
    [Route("api/photos/")]
    [Authorize]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IPhotoService _photoService;
        private readonly IUserService _userService;


        public PhotosController(IPhotoService photoService, IUserService userService)
        {
            _photoService = photoService;
            _userService = userService;
        }
        
        [HttpGet("{userId}")]
        public async Task<IActionResult> DownloadAsync(Guid userId)
        {
            var photo = await _userService.GetAvatarAsync(userId);
            var file = await _photoService.DownloadAsync(photo);

            return File(file.MemoryStream, file.ContentType);
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> UploadAsync(Guid userId, IFormFile file)
        {
            var fileName = await _photoService.SaveAsync(file);

            await _userService.SetAvatarAsync(userId, fileName);
            
            return Ok();
        }
    }
}