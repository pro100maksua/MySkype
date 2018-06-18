using System;
using System.Collections.Generic;
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

        public async Task<PhotoStream> DownloadAsync(Guid userId)
        {
            var user = await _usersRepository.GetAsync(userId);
            var photo = await _photoRepository.GetAsync(user.AvatarId);

            var folder = Path.Combine(_env.WebRootPath, "photos");
            var path = Path.Combine(folder, photo.FileName);

            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memoryStream);
            }

            memoryStream.Position = 0;
            var photoStream = new PhotoStream
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

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var photo = new Photo { Id = Guid.NewGuid(), FileName = fileName };
            await _photoRepository.AddAsync(photo);
            await _usersRepository.SetUserPhotoAsync(userId, photo.Id);

            return photo;

        }

        public async Task<Guid> UploadAsync(Bitmap bitmap)
        {
            var folder = Path.Combine(_env.WebRootPath, "photos");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + ".png";
            var path = Path.Combine(folder, fileName);

            bitmap.Save(path, ImageFormat.Png);

            var photo = new Photo { Id = Guid.NewGuid(), FileName = fileName };
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
