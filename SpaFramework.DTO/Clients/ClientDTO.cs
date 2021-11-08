using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.DTO.Clients
{
    public class ClientDTO : IDTO
    {
        public long Id { get; set; }

        public string ConcurrencyCheck { get; set; }

        public string Name { get; set; }

        public string Abbreviation { get; set; }

        public ClientStatsDTO ClientStats { get; set; }

        public Instant LastModification { get; set; }
        public bool Deleted { get; set; }
    }
}
