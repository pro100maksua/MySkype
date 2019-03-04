using System;
using System.Collections.Generic;

namespace MySkype.Server.Models
{
    public class Call
    {
        public Guid Id { get; set; }

        public long StartTime { get; set; }

        public long Duration { get; set; }

        public IEnumerable<Guid> ParticipantIds { get; set; }
    }
}
