using System;
using System.Threading.Tasks;
using MySkype.Server.Models;

namespace MySkype.Server.Interfaces
{
    public interface IPhotoRepository
    {
        Task AddAsync(Photo photo);
        Task<Photo> GetAsync(Guid id);
    }
}