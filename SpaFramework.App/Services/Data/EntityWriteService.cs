using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using SpaFramework.App.DAL;
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
using SpaFramework.App.Exceptions;
using SpaFramework.App.Utilities;
using SpaFramework.App.Services.Data;
using SpaFramework.App.Models;

namespace SpaFramework.App.Services.Data
{
    public abstract class EntityWriteService<TDataModel, TIdType> : EntityReadService<TDataModel, TIdType>, IEntityWriteService<TDataModel, TIdType>
        where TDataModel : class, IEntity, IHasId<TIdType>, new()
    {
        protected readonly IValidator<TDataModel> _validator;

        public EntityWriteService(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, IValidator<TDataModel> validator, ILogger<EntityWriteService<TDataModel, TIdType>> logger) : base(dbContext, configuration, userManager, logger)
        {
            _validator = validator;
        }

        #region CRUD Operations

        public async Task<TDataModel> Create(ClaimsPrincipal user, TDataModel dataModel)
        {
            return await Create(user, dataModel, new Dictionary<string, object>());
        }

        protected async Task<TDataModel> Create(ClaimsPrincipal user, TDataModel dataModel, Dictionary<string, object> extraData)
        {
            var applicationUser = await GetApplicationUser(user);

            dataModel = await StripNavigationProperties(dataModel);

            if (!await CanCreate(applicationUser, dataModel, extraData))
                throw new ForbiddenException();

            await OnCreating(user, dataModel, extraData);

            _validator.ValidateAndThrow(dataModel);

            _dbContext.Set<TDataModel>().Add(dataModel);
            await _dbContext.SaveChangesAsync();

            await OnCreated(user, dataModel, extraData);

            _logger.LogInformation("Entity {EntityType} {EntityAction}: {EntityValue} ({EntityId}) by {UserName} ({UserId})", typeof(TDataModel).Name, "Created", (dataModel is ILoggableName ? ((ILoggableName)dataModel).LoggableName : dataModel.ToString()), dataModel.GetId(), user.GetUserName(), user.GetUserId());

            return dataModel;
        }

        public async Task<TDataModel> Update(ClaimsPrincipal user, TDataModel dataModel)
        {
            return await Update(user, dataModel, new Dictionary<string, object>());
        }

        protected async Task<TDataModel> Update(ClaimsPrincipal user, TDataModel dataModel, Dictionary<string, object> extraData)
        {
            var applicationUser = await GetApplicationUser(user);

            dataModel = await StripNavigationProperties(dataModel);

            if (!await CanUpdate(applicationUser, dataModel, extraData))
                throw new ForbiddenException();

            TDataModel oldDataModel = await GetItemById(applicationUser, dataModel.GetId(), null);
            _dbContext.Entry(oldDataModel).State = EntityState.Detached;

            await OnUpdating(user, dataModel, oldDataModel, extraData);

            _validator.ValidateAndThrow(dataModel);

            _dbContext.Entry(dataModel).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            await OnUpdated(user, dataModel, oldDataModel, extraData);

            _logger.LogInformation("Entity {EntityType} {EntityAction}: {EntityValue} ({EntityId}) by {UserName} ({UserId})", typeof(TDataModel).Name, "Updated", (dataModel is ILoggableName ? ((ILoggableName)dataModel).LoggableName : dataModel.ToString()), dataModel.GetId(), user.GetUserName(), user.GetUserId());

            return dataModel;
        }

        #endregion

        #region Basic Operation Permissions

        /// <summary>
        /// Returns the list of roles that have write permissions. If null, everyone will have permission. If empty, no one will have permission. Otherwise, only those in this list will have permission.
        /// 
        /// Note that, if this service implements IDonorAccessService, a user always has access to their own donor data, regardless of their other permissions.
        /// 
        /// Note too that outlet-level access is also applied. That is, unless ApplicationUser.AllOutlets is true, only the outlets that the user has access to will be returned.
        /// </summary>
        protected virtual List<string> WriteRoles => null;

        /// <summary>
        /// Returns whether or not a user can perform non-read (create, update, delete) operations on a model. By default, CanCreate, CanUpdate, and CanDelete call this method, but each of them
        /// may provide their own custom implementation that won't call this method. The data model passed in should already contain all the data needed to make a determination on the user's
        /// ability to write the data.
        /// 
        /// The default implementation limits write access in the following way:
        /// * If WriteRoles is null, everyone can write all items
        /// * If WriteRoles is an empty list, no one has write any items
        /// * If the user is not in any of the WriteRoles
        ///   * If this service implements IDonorAccessWriteService and the user is the donor, the user has access to their own donor data
        ///   * Otherwise, the user has no access to any items
        /// * If the user is in one of the WriteRoles
        ///   * If this service implements IOutletAccessWriteService, the user is given access to items associated with their outlet(s). If the user has AllOutlets set, the user can access data from all outlets
        ///   * Otherwise, the user has write access to all items
        /// 
        /// This can be overwritten in derived classes to enforce their own security model
        /// </summary>
        /// <param name="applicationUser"></param>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        protected virtual async Task<bool> CanWrite(ApplicationUser applicationUser, TDataModel dataModel, Dictionary<string, object> extraData)
        {
            if (WriteRoles == null)
                return true;

            if (applicationUser == null)
                return false;

            var roles = await _userManager.GetRolesAsync(applicationUser);

            // If the user doesn't match any roles, they don't have access
            if (!roles.Any(r => WriteRoles.Contains(r)))
                return false;

            // The user has a matching role, and this isn't outlet-specific data, so allow write
            return true;
        }
        
        protected virtual async Task<bool> CanCreate(ApplicationUser applicationUser, TDataModel dataModel, Dictionary<string, object> extraData) { return await CanWrite(applicationUser, dataModel, extraData); }
        protected virtual async Task<bool> CanUpdate(ApplicationUser applicationUser, TDataModel dataModel, Dictionary<string, object> extraData) { return await CanWrite(applicationUser, dataModel, extraData); }

        #endregion

        protected virtual async Task OnCreating(ClaimsPrincipal user, TDataModel dataModel, Dictionary<string, object> extraData) { }
        protected virtual async Task OnUpdating(ClaimsPrincipal user, TDataModel dataModel, TDataModel oldDataModel, Dictionary<string, object> extraData) { }

        protected virtual async Task OnCreated(ClaimsPrincipal user, TDataModel dataModel, Dictionary<string, object> extraData) { }
        protected virtual async Task OnUpdated(ClaimsPrincipal user, TDataModel dataModel, TDataModel oldDataModel, Dictionary<string, object> extraData) { }

        protected virtual async Task<TDataModel> StripNavigationProperties(TDataModel dataModel)
        {
            return SerializationUtilities.CloneModel<TDataModel>(dataModel);
        }

        /// <summary>
        /// Creates linked items -- that is, items in a many-to-many relationship with this entity that can be created as children of this entity. Call this in OnCreating for each linked table.
        /// </summary>
        /// <typeparam name="TLinkedItem"></typeparam>
        /// <typeparam name="TLinkedItemIdType"></typeparam>
        /// <param name="user"></param>
        /// <param name="dataModel"></param>
        /// <param name="dataModelItems">The list of linked items on the dataModel</param>
        /// <param name="linkedItemParentIdSetter">An action that, for a given linked item, sets the parent ID to this entity</param>
        /// <returns></returns>
        protected async Task CreateLinkedItems<TLinkedItem, TLinkedItemIdType>(ClaimsPrincipal user, TDataModel dataModel, List<TLinkedItem> dataModelItems, Action<TLinkedItem, TIdType> linkedItemParentIdSetter)
            where TLinkedItem : IHasId<TLinkedItemIdType>
        {
            if (dataModelItems != null)
            {
                foreach (var item in dataModelItems)
                {
                    linkedItemParentIdSetter(item, dataModel.GetId());
                    _dbContext.Entry(item).State = EntityState.Added;
                }
            }
        }

        /// <summary>
        /// Updates linked items -- that is, items in a many-to-many relationship with this entity that can be created as children of this entity. Call this in OnUpdating for each linked table.
        /// </summary>
        /// <typeparam name="TLinkedItem"></typeparam>
        /// <typeparam name="TLinkedItemIdType"></typeparam>
        /// <param name="user"></param>
        /// <param name="dataModel"></param>
        /// <param name="dataModelItems">The list of linked items on the dataModel</param>
        /// <param name="linkedItemParentIdSetter">An action that, for a given linked item, sets the parent ID to this entity</param>
        /// <param name="existingLinkedItemsGetter">A function that returns all the linked items for a given parent ID</param>
        /// <returns></returns>
        protected async Task<List<TLinkedItemIdType>> UpdateLinkedItems<TLinkedItem, TLinkedItemIdType>(ClaimsPrincipal user, TDataModel dataModel, List<TLinkedItem> dataModelItems, Action<TLinkedItem, TIdType> linkedItemParentIdSetter, Func<ApplicationDbContext, TIdType, IQueryable<TLinkedItem>> existingLinkedItemsGetter)
            where TLinkedItem : class, IHasId<TLinkedItemIdType>
        {
            List<TLinkedItemIdType> deletedIds = new List<TLinkedItemIdType>();

            if (dataModelItems != null)
            {
                List<TLinkedItem> existingItems = await existingLinkedItemsGetter(_dbContext, dataModel.GetId()).ToListAsync();

                foreach (var item in existingItems.Where(x => !dataModelItems.Any(y => y.GetId().Equals(x.GetId()))))
                {
                    deletedIds.Add(item.GetId());
                    _dbContext.Entry(item).State = EntityState.Deleted;
                }

                foreach (var item in dataModelItems.Where(x => !existingItems.Any(y => y.GetId().Equals(x.GetId()))))
                {
                    linkedItemParentIdSetter(item, dataModel.GetId());
                    _dbContext.Entry(item).State = EntityState.Added;
                }

                List<TLinkedItem> ignoreItems = dataModelItems.Where(x => existingItems.Any(y => y.GetId().Equals(x.GetId()))).ToList();
                foreach (var item in ignoreItems)
                    dataModelItems.Remove(item);
            }

            return deletedIds;
        }
    }
}
