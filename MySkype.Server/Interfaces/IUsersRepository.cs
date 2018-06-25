using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySkype.Server.Models;

namespace MySkype.Server.Interfaces
{
    public interface IUsersRepository
    {
        Task<IEnumerable<User>> GetAllAsync(string searchQuery);
        Task<IEnumerable<User>> GetFriendsAsync(User user);
        Task AddFriendAsync(Guid id, Guid friendId);
        Task AddFriendRequestAsync(Guid id, Guid friendId);
        Task RemoveFriendRequestAsync(Guid id, Guid friendId);
        Task<bool> CheckIfFriendAsync(Guid id, Guid friendId);
        Task<User> GetAsync(Guid id);
        Task AddAsync(User user);
        Task<User> GetAsync(string login, string password);
        Task SetUserPhotoAsync(Guid userId, Guid photoId);
        Task<bool> UserExistsAsync(string dtoLogin);
    }
}