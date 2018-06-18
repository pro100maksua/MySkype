using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using MySkype.Server.Dto;
using MySkype.Server.Interfaces;
using MySkype.Server.Models;
using MySkype.Server.WebSocketManagers;

namespace MySkype.Server.Services
{
    public class UserService
    {
        private readonly IWebSocketManager _webSocketManager;
        private readonly IUsersRepository _usersRepository;
        private readonly PhotoService _photoService;
        private readonly IPhotoRepository _photoRepository;
        private readonly IMapper _mapper;

        public UserService(IWebSocketManager webSocketManager, IUsersRepository usersRepository,
            PhotoService photoService, IPhotoRepository photoRepository, IMapper mapper)
        {
            _webSocketManager = webSocketManager;
            _usersRepository = usersRepository;
            _photoService = photoService;
            _photoRepository = photoRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ResponseUserDto>> GetAllAsync(string searchQuery)
        {
            var users = await _usersRepository.GetAllAsync(searchQuery);

            var tasks = users.Select(async u =>
            {
                var dto = _mapper.Map<User, ResponseUserDto>(u);
                dto.Avatar = await _photoRepository.GetAsync(u.AvatarId);
                return dto;
            }).ToList();

            var userDtos = await Task.WhenAll(tasks);

            return userDtos;
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
            var alreadyFriend = await _usersRepository.CheckIfFriendAsync(id, friendId);

            if (alreadyFriend) return false;

            await _webSocketManager.SendAsync(id, friendId, MessageType.FriendRequest);

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

        public async Task<ResponseUserDto> GetAsync(Guid id)
        {
            var user = await _usersRepository.GetAsync(id);

            var userResponseDto = _mapper.Map<User, ResponseUserDto>(user);
            userResponseDto.Avatar = await _photoRepository.GetAsync(user.AvatarId);

            return userResponseDto;
        }

        public async Task<User> PostAsync(RequestUserDto requestUserDto)
        {
            var user = _mapper.Map<RequestUserDto, User>(requestUserDto);

            var photoId = await _photoService.CreateDefaultPhotoAsync(user.FirstName, user.LastName);

            user.Id = Guid.NewGuid();
            user.AvatarId = photoId;

            await _usersRepository.AddAsync(user);

            return user;
        }

        public async Task SendCallRequestAsync(Guid id, Guid friendId)
        {
            await _webSocketManager.SendAsync(id, friendId, MessageType.CallRequest);
        }

        public async Task ConfirmCallAsync(Guid id, Guid friendId)
        {
            await _webSocketManager.SendAsync(id, friendId, MessageType.CallConfirmation);
        }
    }
}
