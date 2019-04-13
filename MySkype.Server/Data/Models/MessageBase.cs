using System;

namespace MySkype.Server.Data.Models
{
    public class MessageBase
    {
        public MessageType MessageType { get; set; }

        public Guid SenderId { get; set; }

        public Guid TargetId { get; set; }

        public string SenderName { get; set; }
    }
}
