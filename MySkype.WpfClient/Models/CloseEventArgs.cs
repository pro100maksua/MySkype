using System;

namespace MySkype.WpfClient.Models
{
    public class CloseEventArgs : EventArgs
    {
        public bool CallAccepted { get; set; }
    }
}