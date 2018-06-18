using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySkype.Server.Services;

namespace MySkype.Server.Controllers
{
    [Route("api/user/friends")]
    [Authorize]
    [ApiController]
    public class UserFriendsController : ControllerBase
    {
        private readonly UserService _userService;

        public UserFriendsController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFriendsAsync()
        {
            var id = new Guid(User.FindFirst("sid").Value);

            var friends = await _userService.GetFriendsAsync(id);

            return Ok(friends);
        }

        [HttpPost("{friendId}")]
        public async Task<IActionResult> SendFriendRequestAsync(Guid friendId)
        {
            var id = new Guid(User.FindFirst("sid").Value);

            var result = await _userService.SendFriendRequestAsync(id, friendId);

            return Ok(result);
        }

        [HttpPost("{friendId}/call")]
        public async Task<IActionResult> SendCallRequestAsync(Guid friendId)
        {
            var id = new Guid(User.FindFirst("sid").Value);

            await _userService.SendCallRequestAsync(id, friendId);

            return Ok();
        }

        [HttpPost("{friendId}/confirmCall")]
        public async Task<IActionResult> ConfirmCallAsync(Guid friendId)
        {
            var id = new Guid(User.FindFirst("sid").Value);

            await _userService.ConfirmCallAsync(id, friendId);

            return Ok();
        }

        [HttpPut("{friendId}")]
        public async Task<IActionResult> ConfirmFriendRequestAsync(Guid friendId)
        {
            var id = new Guid(User.FindFirst("sid").Value);

            var result = await _userService.ConfirmFriendRequestAsync(id, friendId);

            return Ok(result);
        }
    }
}
