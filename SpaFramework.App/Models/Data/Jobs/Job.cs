using Newtonsoft.Json;
using NodaTime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaFramework.App.Models.Data.Jobs
{
    /// <summary>
    /// A record of an automated task performed by the system
    /// </summary>
    public class Job : IEntity, IHasId<long>, ILoggableName
    {
        public long GetId() => this.Id;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [NotMapped]
        public string LoggableName { get { return Name; } }

        [MaxLength(100)]
        public string Name { get; set; }

        public Instant Created { get; set; }
        public Instant? Started { get; set; }
        public Instant? Updated { get; set; }
        public Instant? Ended { get; set; }
        
        public long ExpectedCount { get; set; }
        public long SuccessCount { get; set; }
        public long FailureCount { get; set; }

        [MaxLength(50)]
        public string ItemType { get; set; }

        [NotMapped]
        public List<long> ItemIds { get; set; } = new List<long>();

        [Column("ItemIds")]
        public string SerializedItemIds
        {
            get { return JsonConvert.SerializeObject(ItemIds); }
            set { ItemIds = string.IsNullOrEmpty(value) ? new List<long>() : JsonConvert.DeserializeObject<List<long>>(value); }
        }

        public List<JobItem> JobItems { get; set; }
    }
}
