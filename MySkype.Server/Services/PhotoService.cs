using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MySkype.Server.Interfaces;
using MySkype.Server.Models;

namespace MySkype.Server.Services
{
    public class PhotoService
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IPhotoRepository _photoRepository;
        private readonly IHostingEnvironment _env;

        public PhotoService(IUsersRepository usersRepository, IPhotoRepository photoRepository, IHostingEnvironment env)
        {
            _usersRepository = usersRepository;
            _photoRepository = photoRepository;
            _env = env;
        }

        public async Task<Photo> GetAsync(Guid id)
        {
            return await _photoRepository.GetAsync(id);
        }

        public async Task<PhotoFile> DownloadAsync(Guid id)
        {
            var photo = await _photoRepository.GetAsync(id);

            var folder = Path.Combine(_env.WebRootPath, "photos");
            var path = Path.Combine(folder, photo.FileName);

            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memoryStream);
            }

            memoryStream.Position = 0;
            var photoStream = new PhotoFile
            {
                MemoryStream = memoryStream,
                ContentType = "image/" + Path.GetExtension(photo.FileName)
            };

            return photoStream;
        }

        public async Task<Photo> UploadAsync(Guid userId, IFormFile file)
        {
            var folder = Path.Combine(_env.WebRootPath, "photos");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            
            var photo = new Photo { Id = Guid.NewGuid()};

            photo.FileName = photo.Id + Path.GetExtension(file.FileName);
            var path = Path.Combine(folder, photo.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            await _photoRepository.AddAsync(photo);
            await _usersRepository.SetUserPhotoAsync(userId, photo.Id);

            return photo;

        }

        public async Task<Guid> UploadAsync(Bitmap bitmap)
        {
            var folder = Path.Combine(_env.WebRootPath, "photos");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            
            var photo = new Photo { Id = Guid.NewGuid()};
            photo.FileName = photo.Id + ".png";
            var path = Path.Combine(folder, photo.FileName);

            bitmap.Save(path, ImageFormat.Png);

            await _photoRepository.AddAsync(photo);

            return photo.Id;
        }

        public async Task<Guid> CreateDefaultPhotoAsync(string userFirstName, string userLastName)
        {
            const string fileName = "default.png";
            var folder = Path.Combine(_env.WebRootPath, "photos");
            var path = Path.Combine(folder, fileName);
            var initials = (userFirstName.First().ToString() + userLastName.First()).ToUpper();
            
            var rectangleF = new RectangleF(0, 0, 100, 100);

            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            
            using (var image = new Bitmap(Image.FromFile(path)))
            {
                var graphics = Graphics.FromImage(image);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawString(initials, new Font("Tahoma", 35), Brushes.Black, rectangleF, sf);
                graphics.Flush();

                var photoId = await UploadAsync(image);

                return photoId;
            }
        }
    }
}
