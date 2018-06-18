using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySkype.Server.Models;

namespace MySkype.Server.Interfaces
{
    public interface IPhotoRepository
    {
        Task<IEnumerable<Photo>> GetAllAsync();
        Task AddAsync(Photo photo);
        Task<Photo> GetAsync(Guid id);
    }
}