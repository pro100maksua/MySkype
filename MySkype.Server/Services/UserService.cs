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
    public class UserService
    {
        private readonly WebSocketManager _webSocketManager;
        private readonly IUsersRepository _usersRepository;
        private readonly PhotoService _photoService;
        private readonly IPhotoRepository _photoRepository;
        private readonly IMapper _mapper;

        public UserService(WebSocketManager webSocketManager, IUsersRepository usersRepository,
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
        public async Task<ResponseUserDto> GetAsync(Guid id)
        {
            var user = await _usersRepository.GetAsync(id);

            var dto = _mapper.Map<User, ResponseUserDto>(user);
            dto.Avatar = await _photoRepository.GetAsync(user.AvatarId);

            return dto;
        }
        public async Task<User> PostAsync(RequestUserDto dto)
        {
            var user = _mapper.Map<RequestUserDto, User>(dto);

            var photoId = await _photoService.CreateDefaultPhotoAsync(user.FirstName, user.LastName);

            user.Id = Guid.NewGuid();
            user.AvatarId = photoId;

            await _usersRepository.AddAsync(user);

            return user;
        }

        public async Task<bool> UserExistsAsync(string login)
        {
            return await _usersRepository.UserExistsAsync(login);
        }

        public async Task<bool> UserIsOnlineAsync(Guid userId)
        {
            return await _webSocketManager.CheckIfUserIsOnlineAsync(userId);
        }
    }
}
