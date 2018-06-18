using System.IO;

namespace MySkype.Server.Services
{
    public class PhotoStream
    {
        public MemoryStream MemoryStream { get; set; }

        public string ContentType { get; set; }
    }
}