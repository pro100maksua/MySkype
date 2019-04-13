using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MySkype.Server.Data.Interfaces;
using MySkype.Server.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MySkype.Server.Data.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly MongoContext _context;

        public UsersRepository(MongoContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync(Expression<Func<User, bool>> filter = null)
        {
            var users = _context.Users.AsQueryable();
            
            if (filter != null)
            {
                users = users.Where(filter);
            }

            return await users.ToListAsync();
        }
        
        public async Task AddFriendAsync(Guid id, Guid friendId)
        {
            var update = Builders<User>.Update.AddToSet(u => u.FriendIds, friendId);

            await _context.Users.UpdateOneAsync(u => u.Id == id, update);
        }

        public async Task SetUserAvatarAsync(Guid userId, Photo photo)
        {
            var update = Builders<User>.Update.Set(u => u.Avatar, photo);

            await _context.Users.UpdateOneAsync(u => u.Id == userId, update);
        }

        public async Task<Photo> GetUserAvatarAsync(Guid userId)
        {
            var photo = await _context.Users
                .Find(u => u.Id == userId)
                .Project(u => u.Avatar)
                .FirstOrDefaultAsync();

            return photo;
        }

        public async Task AddFriendRequestAsync(Guid id, Guid friendId)
        {
            var update = Builders<User>.Update.AddToSet(u => u.FriendRequests, friendId);

            await _context.Users.UpdateOneAsync(u => u.Id == id, update);
        }

        public async Task RemoveFriendRequestAsync(Guid id, Guid friendId)
        {
            var update = Builders<User>.Update.Pull(u => u.FriendRequests, friendId);

            await _context.Users.UpdateOneAsync(u => u.Id == id, update);
        }

        public async Task<bool> ExistsAsync(Expression<Func<User, bool>> filter)
        {
            return await _context.Users.AsQueryable().AnyAsync(filter);
        }

        public async Task<User> GetAsync(Guid id)
        {
            return await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }
    }
}