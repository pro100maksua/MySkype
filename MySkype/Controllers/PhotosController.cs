using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySkype.Server.Services;

namespace MySkype.Server.Controllers
{
    [Route("api/users")]
    [Authorize]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly PhotoService _photoService;

        public PhotosController(PhotoService photoService)
        {
            _photoService = photoService;
        }
        
        [HttpGet("{userId}/photo")]
        public async Task<IActionResult> DownloadAsync(Guid userId)
        {
            var photoStream = await _photoService.DownloadAsync(userId);

            return File(photoStream.MemoryStream, photoStream.ContentType);
        }

        [HttpPost("{userId}/photo")]
        public async Task<IActionResult> UploadAsync(Guid userId, IFormFile file)
        {
            var photo = await _photoService.UploadAsync(userId, file);

            return Ok(photo);
        }
    }
}