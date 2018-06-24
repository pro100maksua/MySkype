using System;

namespace MySkype.WpfClient.Models
{
    public class Message
    {
        public Guid SenderId { get; set; }

        public Guid TargetId { get; set; }

        public string SenderName { get; set; }

        public MessageType MessageType { get; set; }
    }
}
