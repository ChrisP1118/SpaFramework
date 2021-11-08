using SpaFramework.Core.Models;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.DTO.Clients
{
    public class ProjectDTO : IDTO
    {
        public long Id { get; set; }

        public string ConcurrencyCheck { get; set; }

        public string Name { get; set; }

        public long ClientId { get; set; }
        public ClientDTO Client { get; set; }

        public LocalDate StartDate { get; set; }
        public LocalDate EndDate { get; set; }

        public ProjectState State { get; set; }

        public Instant LastModification { get; set; }
        public bool Deleted { get; set; }
    }
}
