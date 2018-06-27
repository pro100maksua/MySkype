namespace MySkype.WpfClient.Models
{
    public class Notification : MessageBase
    {
        public NotificationType NotificationType { get; set; }

        public Notification()
        {
            MessageType = MessageType.Notification;
        }
    }
}