using System.Drawing;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MySkype.Server.Data.Models;

namespace MySkype.Server.Logic.Interfaces
{
    public interface IPhotoService
    {
        Task<PhotoFile> DownloadAsync(Photo photo);
        Task<string> SaveAsync(IFormFile file);
        Task<string> SaveAsync(Bitmap bitmap);
        Task<string> CreateDefaultAvatarAsync(string userFirstName, string userLastName);
    }
}