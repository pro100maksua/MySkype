using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySkype.Server.Models;
using MySkype.Server.Services;
using MySkype.Server.WebSocketManagers;

namespace MySkype.Server.Controllers
{
    [Route("api/user/friends")]
    [Authorize]
    [ApiController]
    public class UserFriendsController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IWebSocketManager _webSocketManager;

        public UserFriendsController(UserService userService, IWebSocketManager webSocketManager)
        {
            _userService = userService;
            _webSocketManager = webSocketManager;
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

        [HttpPut("{friendId}")]
        public async Task<IActionResult> ConfirmFriendRequestAsync(Guid friendId)
        {
            var id = new Guid(User.FindFirst("sid").Value);

            var result = await _userService.ConfirmFriendRequestAsync(id, friendId);

            return Ok(result);
        }

        [HttpPost("{friendId}/data")]
        public async Task<IActionResult> SendBytesAsync([FromBody] byte[] data, Guid friendId)
        {
            await _webSocketManager.SendBytesAsync(friendId, data);

            return Ok();
        }
    }
}
