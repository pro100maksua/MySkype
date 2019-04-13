using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySkype.Server.Data.Models;
using MySkype.Server.Logic.Dto;

namespace MySkype.Server.Logic.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllAsync(string searchQuery);
        Task<UserResponseDto> GetAsync(Guid id);
        Task SetAvatarAsync(Guid userId, string fileName);
        Task<Photo> GetAvatarAsync(Guid userId);
        Task<bool> UserIsOnlineAsync(Guid userId);
    }
}