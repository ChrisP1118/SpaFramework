using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.App.Models.Data.ChangeTracking;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaFramework.App.Models.Data
{
    public interface IChangeTracked<T, TId, TTrackedChange> : IHasDeleted
        where T : IHasId<TId>
        where TTrackedChange : TrackedChange<T, TId>
    {
        List<TTrackedChange> TrackedChanges { get; set; }
        Instant LastModification { get; set; }
    }
}
