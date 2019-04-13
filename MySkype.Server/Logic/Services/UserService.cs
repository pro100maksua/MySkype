using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Mapster;
using MySkype.Server.Data.Interfaces;
using MySkype.Server.Data.Models;
using MySkype.Server.Logic.Dto;
using MySkype.Server.Logic.Interfaces;
using MySkype.Server.Logic.WebSocketManagers;

namespace MySkype.Server.Logic.Services
{
    public class UserService : IUserService
    {
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly IUsersRepository _usersRepository;

        public UserService(WebSocketConnectionManager connectionManager, IUsersRepository usersRepository)
        {
            _connectionManager = connectionManager;
            _usersRepository = usersRepository;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllAsync(string searchQuery)
        {
            var isSearchEmpty = string.IsNullOrWhiteSpace(searchQuery);
            Expression<Func<User, bool>> filter = u =>
                isSearchEmpty ||
                u.FirstName.ToUpper().Contains(searchQuery.ToUpper()) ||
                u.LastName.ToUpper().Contains(searchQuery.ToUpper());

            var users = await _usersRepository.GetAllAsync(filter);

            var userDtos = users.Adapt<IEnumerable<User>, IEnumerable<UserResponseDto>>();

            return userDtos;
        }
        public async Task<UserResponseDto> GetAsync(Guid id)
        {
            var user = await _usersRepository.GetAsync(id);

            var dto = user.Adapt<User, UserResponseDto>();

            return dto;
        }

        public async Task SetAvatarAsync(Guid userId, string fileName)
        {
            var photo = new Photo { Id = Guid.NewGuid(), FileName = fileName };

            await _usersRepository.SetUserAvatarAsync(userId, photo);
        }

        public async Task<Photo> GetAvatarAsync(Guid userId)
        {
            var photo = await _usersRepository.GetUserAvatarAsync(userId);

            return photo;
        }

        public Task<bool> UserIsOnlineAsync(Guid userId)
        {
            var socket = _connectionManager.GetSocket(userId);

            var isOnline = socket != null && socket.State == WebSocketState.Open;

            return Task.FromResult(isOnline);
        }
    }
}
