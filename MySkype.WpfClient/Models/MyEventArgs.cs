using System;

namespace MySkype.WpfClient.Models
{
    public class MyEventArgs : EventArgs
    {
        public Guid SenderId { get; set; }
        public byte[] Data { get; set; }
    }
}