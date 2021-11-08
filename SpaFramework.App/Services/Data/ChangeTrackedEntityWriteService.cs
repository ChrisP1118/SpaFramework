using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using SpaFramework.App.DAL;
using SpaFramework.App.Exceptions;
using SpaFramework.App.Models.Data;
using SpaFramework.App.Models.Data.Accounts;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Models.Data.ChangeTracking;
using SpaFramework.App.Utilities;
using Newtonsoft.Json;
using NodaTime;

namespace SpaFramework.App.Services.Data
{
    /// <summary>
    /// A service class for working with entities that have change tracking. That is, every change is recorded, along with an audit of the changes.
    /// </summary>
    /// <typeparam name="TDataModel"></typeparam>
    /// <typeparam name="TIdType"></typeparam>
    /// <typeparam name="TTrackedChange"></typeparam>
    public abstract class ChangeTrackedEntityWriteService<TDataModel, TIdType, TTrackedChange> : EntityWriteService<TDataModel, TIdType>
        where TDataModel : class, IEntity, IHasId<TIdType>, IChangeTracked<TDataModel, TIdType, TTrackedChange>, new()
        where TTrackedChange : TrackedChange<TDataModel, TIdType>, new()
    {
        private readonly IClock _clock;
        private static JsonSerializerSettings _serializerSettings;

        public ChangeTrackedEntityWriteService(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, IValidator<TDataModel> validator, IClock clock, ILogger<ChangeTrackedEntityWriteService<TDataModel, TIdType, TTrackedChange>> logger) : base(dbContext, configuration, userManager, validator, logger)
        {
            _clock = clock;
        }

        private static JsonSerializerSettings SerializerSettings
        {
            get
            {
                if (_serializerSettings == null)
                    _serializerSettings = new JsonSerializerSettings()
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.None,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };

                return _serializerSettings;
            }
        }

        protected override async Task OnCreating(ClaimsPrincipal user, TDataModel dataModel, Dictionary<string, object> extraData)
        {
            Instant currentInstant = _clock.GetCurrentInstant();

            if (dataModel.TrackedChanges == null)
                dataModel.TrackedChanges = new List<TTrackedChange>();

            string newValue = JsonConvert.SerializeObject(SerializationUtilities.CloneModel(dataModel), SerializerSettings);

            dataModel.TrackedChanges.Add(new TTrackedChange()
            {
                Timestamp = currentInstant,
                EntityId = dataModel.GetId(),
                ApplicationUserId = user.GetUserIdAsLong(),
                NewValue = newValue
            });

            dataModel.LastModification = currentInstant;

            await base.OnCreating(user, dataModel, extraData);
        }

        protected override async Task OnUpdating(ClaimsPrincipal user, TDataModel dataModel, TDataModel oldDataModel, Dictionary<string, object> extraData)
        {
            Instant currentInstant = _clock.GetCurrentInstant();

            if (dataModel.TrackedChanges == null)
                dataModel.TrackedChanges = new List<TTrackedChange>();

            dataModel.LastModification = currentInstant;

            string oldValue = JsonConvert.SerializeObject(SerializationUtilities.CloneModel(oldDataModel), SerializerSettings);
            string newValue = JsonConvert.SerializeObject(SerializationUtilities.CloneModel(dataModel), SerializerSettings);

            TTrackedChange trackedChange = new TTrackedChange()
            {
                Timestamp = currentInstant,
                EntityId = dataModel.GetId(),
                ApplicationUserId = user.GetUserIdAsLong(),
                OldValue = oldValue,
                NewValue = newValue
            };

            _dbContext.Entry(trackedChange).State = EntityState.Added;

            dataModel.TrackedChanges.Add(trackedChange);

            await base.OnUpdating(user, dataModel, oldDataModel, extraData);
        }
    }
}