using System;

namespace MySkype.WpfClient.Models
{
    public class AuthEventArgs : EventArgs
    {
        public string Token { get; set; }
    }
}