using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MySkype.Server.Interfaces;
using MySkype.Server.Models;

namespace MySkype.Server.Repositories
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly MongoContext _context;

        public PhotoRepository(MongoContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Photo>> GetAllAsync()
        {
            var photos = await _context.Photos.AsQueryable().ToListAsync();

            return photos;
        }

        public async Task<Photo> GetAsync(Guid id)
        {
            var photo = await _context.Photos
                .AsQueryable()
                .FirstOrDefaultAsync(u => u.Id == id);

            return photo;
        }

        public async Task AddAsync(Photo photo)
        {
            await _context.Photos.InsertOneAsync(photo);
        }
    }
}