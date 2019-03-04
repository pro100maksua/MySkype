using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySkype.Server.Dto;
using MySkype.Server.Services;

namespace MySkype.Server.Controllers
{
    [Route("api/users")]
    [Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] string searchString)
        {
            var userId = new Guid(User.FindFirst("sid").Value);

            var users = await _userService.GetAllAsync(searchString);

            return Ok(users.Where(u => u.Id != userId));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var user = await _userService.GetAsync(id);

            return Ok(user);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> PostAsync([FromBody] RegisterRequest requestUserDto)
        {
            if (await _userService.UserExistsAsync(requestUserDto.Login))
            {
                return BadRequest();
            }

            var user = await _userService.PostAsync(requestUserDto);

            return Ok(user);
        }

        [HttpGet("{userId}/isOnline")]
        public async Task<IActionResult> CheckIfUserIsOnlineAsync(Guid userId)
        {
            var isOnline = await _userService.UserIsOnlineAsync(userId);

            return Ok(isOnline);
        }
    }
}