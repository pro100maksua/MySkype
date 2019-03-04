using System.Windows.Media.Imaging;

namespace MySkype.WpfClient.Models
{
    public class ChatMessage
    {
        public string Content { get; set; }

        public string UserName { get; set; }

        public BitmapImage UserAvatar { get; set; }
    }
}