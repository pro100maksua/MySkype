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
using MySkype.Server.Data.Models;
using MySkype.Server.Logic.Interfaces;

namespace MySkype.Server.Logic.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IHostingEnvironment _env;

        public PhotoService(IHostingEnvironment env)
        {
            _env = env;
        }

        public async Task<PhotoFile> DownloadAsync(Photo photo)
        {
            var path = GetFilePath(photo.FileName);

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


        public async Task<string> SaveAsync(IFormFile file)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = GetFilePath(fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public Task<string> SaveAsync(Bitmap bitmap)
        {
            var fileName = Guid.NewGuid() + ".png";
            var path = GetFilePath(fileName);

            bitmap.Save(path, ImageFormat.Png);

            return Task.FromResult(fileName);
        }

        public async Task<string> CreateDefaultAvatarAsync(string userFirstName, string userLastName)
        {
            var initials = (userFirstName.First().ToString() + userLastName.First()).ToUpper();
            var rectangleF = new RectangleF(0, 0, 100, 100);
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var path = GetFilePath("default.png");
            using (var image = new Bitmap(Image.FromFile(path)))
            {
                var graphics = Graphics.FromImage(image);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawString(initials, new Font("Tahoma", 35), Brushes.DodgerBlue, rectangleF, sf);
                graphics.Flush();

                var fileName = await SaveAsync(image);

                return fileName;
            }
        }

        private string GetFilePath(string fileName)
        {
            var folder = Path.Combine(_env.WebRootPath, "photos");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var path = Path.Combine(folder, fileName);
            return path;
        }
    }
}
