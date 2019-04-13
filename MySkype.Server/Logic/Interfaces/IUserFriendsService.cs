using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySkype.Server.Logic.Dto;

namespace MySkype.Server.Logic.Interfaces
{
    public interface IUserFriendsService
    {
        Task<IEnumerable<UserResponseDto>> GetFriendsAsync(Guid id);
        Task<bool> SendFriendRequestAsync(Guid id, Guid friendId);
        Task<bool> ConfirmFriendRequestAsync(Guid id, Guid friendId);
    }
}