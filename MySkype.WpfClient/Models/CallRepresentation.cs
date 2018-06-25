using System;
using System.Windows.Media.Imaging;

namespace MySkype.WpfClient.Models
{
    public class CallRepresentation
    {
        public Guid UserId { get; set; }

        public string UserFullName { get; set; }

        public DateTime StartTime { get; set; }

        public TimeSpan Duration { get; set; }

        public BitmapImage UserAvatar { get; set; }
    }
}
