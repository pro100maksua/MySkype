using System;

namespace MySkype.Client.Models
{
    public class Message
    {
        public Guid SenderId { get; set; }

        public string SenderName { get; set; }

        public MessageType MessageType { get; set; }
    }
}
