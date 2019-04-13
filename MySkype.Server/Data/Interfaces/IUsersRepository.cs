using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MySkype.Server.Data.Models;

namespace MySkype.Server.Data.Interfaces
{
    public interface IUsersRepository
    {
        Task<IEnumerable<User>> GetAllAsync(Expression<Func<User, bool>> filter = null);
        Task AddFriendAsync(Guid id, Guid friendId);
        Task AddFriendRequestAsync(Guid id, Guid friendId);
        Task RemoveFriendRequestAsync(Guid id, Guid friendId);
        Task<User> GetAsync(Guid id);
        Task<bool> ExistsAsync(Expression<Func<User, bool>> filter);
        Task SetUserAvatarAsync(Guid userId, Photo photo);
        Task<Photo> GetUserAvatarAsync(Guid userId);
    }
}