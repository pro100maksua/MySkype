using System;

namespace MySkype.Server.Models
{
    public class Call
    {
        public Guid Id { get; set; }

        public long Duration { get; set; }
    }
}
