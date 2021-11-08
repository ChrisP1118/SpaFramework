using NodaTime;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaFramework.App.Models.Data.Jobs
{
    /// <summary>
    /// A record of an automated task performed by the system
    /// </summary>
    public class JobItem : IEntity, IHasId<long>
    {
        public long GetId() => this.Id;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long JobId { get; set; }
        public Job Job { get; set; }

        public long? ItemId { get; set; }

        public Instant Timestamp { get; set; }
        
        public string Note { get; set; }
    }
}
