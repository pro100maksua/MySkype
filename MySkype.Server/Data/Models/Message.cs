namespace MySkype.Server.Data.Models
{
    public class Message : MessageBase
    {
        public string Content { get; set; }

        public Message()
        {
            MessageType = MessageType.Message;
        }
    }
}
