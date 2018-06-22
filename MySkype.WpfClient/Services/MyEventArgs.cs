using System;

namespace MySkype.WpfClient.Services
{
    public class MyEventArgs : EventArgs
    {
        public Guid SenderId { get; set; }
    }
}