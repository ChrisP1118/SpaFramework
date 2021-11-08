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
    public abstract class EntityListService<TDataModel> : IEntityListService<TDataModel>
        where TDataModel : class, IEntity
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly IConfiguration _configuration;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly ILogger<EntityListService<TDataModel>> _logger;

        protected static ParsingConfig _parsingConfig;
        protected static ParsingConfig GetParsingConfig()
        {
            if (_parsingConfig == null)
            {
                _parsingConfig = new ParsingConfig();
                _parsingConfig.CustomTypeProvider = new CustomDynamicLinqProvider();
            }

            return _parsingConfig;
        }

        public EntityListService(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, ILogger<EntityListService<TDataModel>> logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _userManager = userManager;
            _logger = logger;
        }

        #region CRUD Operations

        public async Task<List<TDataModel>> GetAll(ClaimsPrincipal user, int offset, int limit, string order, string includes, string filter, bool noTracking = false, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> filterFunc = null, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> includeFunc = null)
        {
            var applicationUser = await GetApplicationUser(user);

            IQueryable<TDataModel> queryable = (await GetDataSet(applicationUser));

            queryable = await ApplyIncludes(applicationUser, queryable, includes, includeFunc);

            if (!string.IsNullOrEmpty(filter))
                queryable = queryable.Where(GetParsingConfig(), filter);

            if (filterFunc != null)
                queryable = filterFunc(queryable);

            if (!string.IsNullOrEmpty(order))
                queryable = queryable.OrderBy(order);

            queryable = queryable
                .Skip(offset)
                .Take(limit);

            if (noTracking)
                queryable = queryable.AsNoTracking();

            List<TDataModel> dataModelItems = await queryable.ToListAsync();

            return dataModelItems;
        }

        public async Task<long> GetAllCount(ClaimsPrincipal user, string filter, int maxCount, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> filterFunc = null)
        {
            var applicationUser = await GetApplicationUser(user);

            IQueryable<TDataModel> queryable = (await GetDataSet(applicationUser));

            if (!string.IsNullOrEmpty(filter))
                queryable = queryable.Where(GetParsingConfig(), filter);

            if (filterFunc != null)
                queryable = filterFunc(queryable);

            if (maxCount > 0)
                queryable = queryable.Take(maxCount);

            long count = await queryable.LongCountAsync();

            return count;
        }

        #endregion

        #region Basic Operation Permissions

        /// <summary>
        /// Returns the list of roles that have read permissions. If null, everyone will have permission. If empty, no one will have permission. Otherwise, only those in this list will have permission.
        /// 
        /// Note that, if this service implements IDonorAccessService, a user always has access to their own donor data, regardless of their other permissions.
        /// 
        /// Note too that outlet-level filtering is also applied. That is, unless ApplicationUser.AllOutlets is true, only the outlets that the user has access to will be returned.
        /// </summary>
        protected virtual List<string> ReadRoles => null;

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
        protected virtual async Task<IQueryable<TDataModel>> ApplyReadSecurity(ApplicationUser applicationUser, IQueryable<TDataModel> queryable)
        {
            if (ReadRoles == null)
                return queryable;

            if (applicationUser == null)
                return queryable.Where(x => false);

            var roles = await _userManager.GetRolesAsync(applicationUser);

            // If the user doesn't match any roles, they don't have access
            if (!roles.Any(r => ReadRoles.Contains(r)))
                return queryable.Where(x => false);

            // The user has a matching role, and this isn't outlet-specific data, so return it all
            return queryable;
        }

        #endregion

        /// <summary>
        /// Returns an ApplicationUser, with necessary security attributes, for a ClaimsPrincipal
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected async Task<ApplicationUser> GetApplicationUser(ClaimsPrincipal user)
        {
            if (user == null)
                return null;

            string userIdString = _userManager.GetUserId(user);
            if (string.IsNullOrEmpty(userIdString))
                return null;

            long userId;
            if (!long.TryParse(userIdString, out userId))
                return null;

            return await _dbContext.ApplicationUsers
                .Where(au => au.Id == userId)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Gets the underlying dataset for the data model with read security applied
        /// </summary>
        /// <param name="applicationUser"></param>
        /// <returns></returns>
        protected virtual async Task<IQueryable<TDataModel>> GetDataSet(ApplicationUser applicationUser)
        {
            IQueryable<TDataModel> queryable = _dbContext.Set<TDataModel>();

            if (typeof(IHasDeleted).IsAssignableFrom(typeof(TDataModel)))
                queryable = queryable.Where(x => !((IHasDeleted)x).Deleted);

            return await ApplyReadSecurity(applicationUser, queryable);
        }

        protected virtual async Task<IQueryable<TDataModel>> ApplyIncludes(ApplicationUser applicationUser, IQueryable<TDataModel> queryable, string includes, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> includesFunc = null)
        {
            if (!string.IsNullOrEmpty(includes))
            {
                foreach (string include in includes.Split(","))
                {
                    // Break this apart at the dots, and ensure that each character after a dot is upper case -- this converts it from camelCase to PascalCase. There's probably a regex that could do this more efficiently. But ugh, regex.
                    string[] parts = include.Split(".");
                    string pascalCasedInclude = string.Join(".", parts.Select(x => x.Substring(0, 1).ToUpper() + x.Substring(1)));
                    if (!await CanInclude(applicationUser, pascalCasedInclude))
                        throw new ForbiddenException("User does not have permissions to include " + pascalCasedInclude);
                    queryable = queryable.Include(pascalCasedInclude);
                }
            }

            if (includesFunc != null)
                queryable = includesFunc(queryable);

            return queryable;
        }

        protected virtual async Task<bool> CanInclude(ApplicationUser applicationUser, string include)
        {
            return true;
        }
    }
}
