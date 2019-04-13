using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mapster;
using MySkype.Server.Data.Interfaces;
using MySkype.Server.Data.Models;
using MySkype.Server.Logic.Dto;
using MySkype.Server.Logic.Interfaces;
using MySkype.Server.Logic.WebSocketManagers;

namespace MySkype.Server.Logic.Services
{
    public class UserFriendsService : IUserFriendsService
    {
        private readonly IUsersRepository _usersRepository;
        private readonly WebSocketManager _webSocketManager;

        public UserFriendsService(IUsersRepository usersRepository, WebSocketManager webSocketManager)
        {
            _usersRepository = usersRepository;
            _webSocketManager = webSocketManager;
        }

        public async Task<IEnumerable<UserResponseDto>> GetFriendsAsync(Guid userId)
        {
            var user = await _usersRepository.GetAsync(userId);
            Expression<Func<User, bool>> filter = u => user.FriendIds.Contains(u.Id);

            var friends = await _usersRepository.GetAllAsync(filter);

            var friendDtos = friends.Adapt<IEnumerable<User>, IEnumerable<UserResponseDto>>();

            return friendDtos;
        }

        public async Task<bool> SendFriendRequestAsync(Guid id, Guid friendId)
        {
            var isFriend = await _usersRepository.ExistsAsync(u => u.Id == id && u.FriendIds.Contains(friendId));
            if (isFriend)
            {
                return false;
            }

            var notification = new Notification
            {
                NotificationType = NotificationType.FriendRequest,
                SenderId = id,
                TargetId = friendId
            };

            await _webSocketManager.SendAsync(notification);

            await _usersRepository.AddFriendRequestAsync(friendId, id);

            return true;
        }

        public async Task<bool> ConfirmFriendRequestAsync(Guid id, Guid friendId)
        {
            var alreadyFriend = await _usersRepository.ExistsAsync(u => u.Id == id && u.FriendIds.Contains(friendId));
            if (alreadyFriend)
            {
                return false;
            }

            await _usersRepository.AddFriendAsync(id, friendId);
            await _usersRepository.AddFriendAsync(friendId, id);
            await _usersRepository.RemoveFriendRequestAsync(id, friendId);

            return true;

        }
    }
}