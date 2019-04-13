using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySkype.Server.Logic.Interfaces;

namespace MySkype.Server.Controllers
{
    [Route("api/users")]
    [Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] string searchString)
        {
            var users = await _userService.GetAllAsync(searchString);

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var user = await _userService.GetAsync(id);

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