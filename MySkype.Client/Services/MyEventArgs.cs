using System;

namespace MySkype.Client.Services
{
    public class MyEventArgs : EventArgs
    {
        public Guid SenderId { get; set; }
    }
}