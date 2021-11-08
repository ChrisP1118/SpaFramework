using SpaFramework.App.Models.Data.ChangeTracking;
using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Models.Data.Clients
{
    public class ClientTrackedChange : TrackedChange<Client, long> { }

    /// <summary>
    /// A communications channel through which a donor or schedule can be created or modified
    /// </summary>
    public class Client : IEntity, IHasId<long>, ILoggableName, IChangeTracked<Client, long, ClientTrackedChange>
    {
        public long GetId() => this.Id;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Timestamp]
        [ConcurrencyCheck]
        public byte[] ConcurrencyTimestamp { get; set; }

        [NotMapped]
        public string ConcurrencyCheck
        {
            get { return ConcurrencyTimestamp == null ? null : Convert.ToBase64String(ConcurrencyTimestamp); }
            set { ConcurrencyTimestamp = value == null ? null : Convert.FromBase64String(value); }
        }

        [NotMapped]
        public string LoggableName { get { return Name; } }

        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(10)]
        public string Abbreviation { get; set; }

        public List<Project> Projects { get; set; }
        public ClientStats ClientStats { get; set; }

        public List<ClientTrackedChange> TrackedChanges { get; set; }
        public Instant LastModification { get; set; }
        public bool Deleted { get; set; }
    }
}
