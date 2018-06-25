using System.IO;

namespace MySkype.Server.Models
{
    public class PhotoFile
    {
        public MemoryStream MemoryStream { get; set; }

        public string ContentType { get; set; }
    }
}