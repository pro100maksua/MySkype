using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySkype.Server.Services;

namespace MySkype.Server.Controllers
{
    [Route("api/photos/")]
    [Authorize]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly PhotoService _photoService;

        public PhotosController(PhotoService photoService)
        {
            _photoService = photoService;
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadAsync(Guid id)
        {
            var file = await _photoService.DownloadAsync(id);

            return File(file.MemoryStream, file.ContentType);
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> UploadAsync(Guid userId, IFormFile file)
        {
            var photo = await _photoService.UploadAsync(userId, file);

            return Ok(photo);
        }
    }
}