using System;
using System.Windows.Media.Imaging;

namespace MySkype.WpfClient.Models
{
    public class CallRepresentation
    {
        public DateTime StartTime { get; set; }

        public TimeSpan Duration { get; set; }

        public BitmapImage Avatar { get; set; }
    }
}
