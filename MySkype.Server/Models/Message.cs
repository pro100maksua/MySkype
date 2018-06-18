using System;

namespace MySkype.Server.Models
{
    public class Message
    {
        public Guid SenderId { get; set; }

        public MessageType MessageType { get; set; }
    }
}
