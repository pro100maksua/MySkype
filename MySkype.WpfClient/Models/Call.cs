using System;
using System.Collections.Generic;

namespace MySkype.WpfClient.Models
{
    public class Call
    {
        public Guid Id { get; set; }

        public long StartTime { get; set; }

        public long Duration { get; set; }

        public List<Guid> ParticipantIds { get; set; }
    }
}
