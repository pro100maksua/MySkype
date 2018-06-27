namespace MySkype.Server.Models
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