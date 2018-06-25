using System;
using System.Collections.Generic;
using System.Linq;

namespace MySkype.WpfClient.Models
{
    public class LargeArea
    {
        public User User { get; set; }

        public List<IGrouping<DateTime, CallRepresentation>> Calls { get; set; }
    }
}