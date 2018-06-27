namespace MySkype.WpfClient.Models
{
    public class Data : MessageBase
    {
        public byte[] Bytes { get; set; }

        public Data()
        {
            MessageType = MessageType.Data;
        }
    }
}