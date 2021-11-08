using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.DTO.Clients
{
    public class ClientStatsDTO : IDTO
    {
        public long ClientId { get; set; }
        public ClientDTO Client { get; set; }

        public int NumberOfProjects { get; set; }
        public LocalDate FirstStartDate { get; set; }
        public LocalDate LastEndDate { get; set; }
    }
}
