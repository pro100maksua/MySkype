using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MySkype.Server.Interfaces;
using MySkype.Server.Models;

namespace MySkype.Server.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly MongoContext _context;

        public UsersRepository(MongoContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync(string searchQuery)
        {
            var users = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
                users = users.Where(u =>
                    u.FirstName.ToLower().Contains(searchQuery.ToLower()) ||
                    u.LastName.ToLower().Contains(searchQuery.ToLower()));

            return await users.ToListAsync();
        }

        public async Task<IEnumerable<User>> GetFriendsAsync(User user)
        {
            var friends = await _context.Users
                 .AsQueryable()
                 .Where(f => user.FriendIds.Contains(f.Id))
                 .ToListAsync();

            return friends;
        }

        public async Task AddFriendAsync(Guid id, Guid friendId)
        {
            var update = Builders<User>.Update.AddToSet(u => u.FriendIds, friendId);

            await _context.Users.UpdateOneAsync(u => u.Id == id, update);
        }

        public async Task SetUserPhotoAsync(Guid userId, Guid photoId)
        {
            var update = Builders<User>.Update.Set(u => u.AvatarId, photoId);

            await _context.Users.UpdateOneAsync(u => u.Id == userId, update);
        }

        public async Task<bool> UserExistsAsync(string login)
        {
            return await _context.Users.Find(u => u.Login == login).AnyAsync();
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

        public async Task<bool> CheckIfFriendAsync(Guid id, Guid friendId)
        {
            var user = await GetAsync(friendId);

            return user.FriendIds.Contains(id);
        }

        public async Task<User> GetAsync(Guid id)
        {
            return await _context.Users
                .AsQueryable()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.InsertOneAsync(user);
        }

        public async Task<User> GetAsync(string login, string password)
        {
            return await _context.Users.Find(u => u.Login.Equals(login) && u.Password.Equals(password)).FirstOrDefaultAsync();
        }
    }
}
