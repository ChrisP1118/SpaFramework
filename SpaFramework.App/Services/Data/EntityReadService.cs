using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using Microsoft.Extensions.Logging;
using SpaFramework.App.DAL;
using SpaFramework.App.Exceptions;
using SpaFramework.App.Models.Data;
using SpaFramework.App.Models.Data.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Services.Data;
using SpaFramework.App.Models;

namespace SpaFramework.App.Services.Data
{
    public abstract class EntityReadService<TDataModel, TIdType> : EntityListService<TDataModel>, IEntityReadService<TDataModel, TIdType>
        where TDataModel : class, IEntity, IHasId<TIdType>
    {
        public EntityReadService(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, ILogger<EntityReadService<TDataModel, TIdType>> logger)
            : base (dbContext, configuration, userManager, logger)
        {
        }

        #region CRUD Operations

        public async Task<TDataModel> GetOne(ClaimsPrincipal user, TIdType id, string includes, bool detach = false, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> includesFunc = null)
        {
            var applicationUser = await GetApplicationUser(user);

            TDataModel dataModel = await GetItemById(applicationUser, id, includes, includesFunc);

            if (detach)
                _dbContext.Entry(dataModel).State = EntityState.Detached;

            return dataModel;
        }

        #endregion

        #region Basic Operation Permissions

        /// <summary>
        /// Returns a queryable with any security-related read filtering applied.
        /// 
        /// The default implementation limits read access in the following way:
        /// * If ReadRoles is null, everyone has access to all items
        /// * If ReadRoles is an empty list, no one has access to any items
        /// * If the user is not in any of the ReadRoles
        ///   * If this service implements IDonorAccessReadService and the user is the donor, the user has access to their own donor data
        ///   * Otherwise, the user has no access to any items
        /// * If the user is in one of the ReadRoles
        ///   * If this service implements IOutletAccessReadService, the user is given access to items associated with their outlet(s). If the user has AllOutlets set, this returns all outlets
        ///   * Otherwise, the user has access to all items
        /// 
        /// This can be overwritten in derived classes to enforce their own security model
        /// </summary>
        /// <param name="applicationUser"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        protected override async Task<IQueryable<TDataModel>> ApplyReadSecurity(ApplicationUser applicationUser, IQueryable<TDataModel> queryable)
        {
            if (ReadRoles == null)
                return queryable;

            if (applicationUser == null)
                return queryable.Where(x => false);

            var roles = await _userManager.GetRolesAsync(applicationUser);

            // If the user doesn't match any read roles, they don't have access
            if (!roles.Any(r => ReadRoles.Contains(r)))
                return queryable.Where(x => false);

            // The user has a matching role, and this isn't outlet-specific data, so return it all
            return queryable;
        }

        #endregion

        /// <summary>
        /// Gets an item by ID, ensuring the user has access to it
        /// </summary>
        /// <param name="applicationUser"></param>
        /// <param name="id"></param>
        /// <param name="includes"></param>
        /// <param name="includesFunc"></param>
        /// <returns></returns>
        protected async Task<TDataModel> GetItemById(ApplicationUser applicationUser, TIdType id, string includes, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> includesFunc = null)
        {
            IQueryable<TDataModel> queryable = await GetDataSet(applicationUser);
            queryable = await ApplyIncludes(applicationUser, queryable, includes, includesFunc);
            queryable = await ApplyIdFilter(queryable, id);
            return await queryable.SingleOrDefaultAsync();
        }

        protected abstract Task<IQueryable<TDataModel>> ApplyIdFilter(IQueryable<TDataModel> queryable, TIdType id);
    }
}
