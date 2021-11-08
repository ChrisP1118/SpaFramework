using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.App.Models.Data.Donors;
using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Models.Data.ChangeTracking
{
    public abstract class TrackedChange<T, TId>
        where T : IHasId<TId>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public TId EntityId { get; set; }
        public T Entity { get; set; }

        public Instant Timestamp { get; set; }

        public long? ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
