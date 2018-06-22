using System;
using System.Windows.Media.Imaging;

namespace MySkype.WpfClient.Models
{
    public class Photo
    {
        public Guid Id { get; set; }

        public string FileName { get; set; }

        public BitmapImage Bitmap { get; set; }
    }
}