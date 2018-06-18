using Avalonia.Media.Imaging;

namespace MySkype.Client2.Models
{
    public class Photo
    {
        public string FileName { get; set; }

        public IBitmap Bitmap { get; set; }
    }
}