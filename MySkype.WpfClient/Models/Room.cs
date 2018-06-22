using System;
using System.Collections.Generic;

namespace MySkype.WpfClient.Models
{
    public class Room
    {
        public Guid Id { get; set; }

        public Photo Photo { get; set; }

        public IEnumerable<Guid> UserIds { get; set; }

        public IEnumerable<Call> Calls { get; set; }

        public string Name { get; set; }
    }
}
