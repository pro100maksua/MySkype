using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySkype.Server.Logic.Interfaces;

namespace MySkype.Server.Controllers
{
    [Route("api/user/friends")]
    [Authorize]
    [ApiController]
    public class UserFriendsController : ControllerBase
    {
        private readonly IUserFriendsService _userFriendsService;

        public UserFriendsController(IUserFriendsService userFriendsService)
        {
            _userFriendsService = userFriendsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFriendsAsync()
        {
            var id = new Guid(User.Identity.Name);

            var friends = await _userFriendsService.GetFriendsAsync(id);

            return Ok(friends);
        }

        [HttpPost("{friendId}")]
        public async Task<IActionResult> SendFriendRequestAsync(Guid friendId)
        {
            var id = new Guid(User.Identity.Name);

            var result = await _userFriendsService.SendFriendRequestAsync(id, friendId);

            return Ok(result);
        }

        [HttpPut("{friendId}")]
        public async Task<IActionResult> ConfirmFriendRequestAsync(Guid friendId)
        {
            var id = new Guid(User.Identity.Name);

            var result = await _userFriendsService.ConfirmFriendRequestAsync(id, friendId);

            return Ok(result);
        }
    }
}