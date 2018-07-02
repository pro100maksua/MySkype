using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MySkype.Server.Dto;
using MySkype.Server.Interfaces;
using MySkype.Server.Models;
using MySkype.Server.WebSocketManagers;

namespace MySkype.Server.Services
{
    public class UserFriendsService
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IPhotoRepository _photoRepository;
        private readonly WebSocketManager _webSocketManager;
        private readonly IMapper _mapper;

        public UserFriendsService(IUsersRepository usersRepository, IPhotoRepository photoRepository, IMapper mapper, WebSocketManager webSocketManager)
        {
            _usersRepository = usersRepository;
            _photoRepository = photoRepository;
            _webSocketManager = webSocketManager;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ResponseUserDto>> GetFriendsAsync(Guid id)
        {
            var user = await _usersRepository.GetAsync(id);
            var friends = await _usersRepository.GetFriendsAsync(user);

            var tasks = friends.Select(async f =>
            {
                var dto = _mapper.Map<User, ResponseUserDto>(f);
                dto.Avatar = await _photoRepository.GetAsync(f.AvatarId);
                return dto;
            }).ToList();

            var friendDtos = await Task.WhenAll(tasks);

            return friendDtos;
        }

        public async Task<bool> SendFriendRequestAsync(Guid id, Guid friendId)
        {
            var isFriend = await _usersRepository.CheckIfFriendAsync(id, friendId);

            if (isFriend) return false;

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
            var alreadyFriend = await _usersRepository.CheckIfFriendAsync(id, friendId);

            if (alreadyFriend) return false;

            await _usersRepository.AddFriendAsync(id, friendId);
            await _usersRepository.AddFriendAsync(friendId, id);
            await _usersRepository.RemoveFriendRequestAsync(id, friendId);

            return true;

        }
    }
}