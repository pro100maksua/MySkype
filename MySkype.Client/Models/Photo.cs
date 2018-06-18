using Avalonia.Media.Imaging;

namespace MySkype.Client.Models
{
    public class Photo
    {
        public string FileName { get; set; }

        public IBitmap Bitmap { get; set; }
    }
}