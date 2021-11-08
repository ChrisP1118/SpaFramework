using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.DTO.Jobs
{
    public class JobDTO : IDTO
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public Instant Created { get; set; }
        public Instant? Started { get; set; }
        public Instant? Updated { get; set; }
        public Instant? Ended { get; set; }

        public long ExpectedCount { get; set; }
        public long SuccessCount { get; set; }
        public long FailureCount { get; set; }

        public string ItemType { get; set; }

        public List<long> ItemIds { get; set; } = new List<long>();
    }
}
