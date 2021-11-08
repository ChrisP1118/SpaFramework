using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SpaFramework.App.DAL;
using SpaFramework.App.Models.Data;
using SpaFramework.App.Models.Data.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Models;
using SpaFramework.Core.Models;
using FluentValidation;
using System.Security.Claims;
using SpaFramework.App.Utilities;

namespace SpaFramework.App.Services.Data.Accounts
{
    public class ApplicationUserService : EntityWriteService<ApplicationUser, long>
    {
        public ApplicationUserService(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, IValidator<ApplicationUser> validator, ILogger<EntityWriteService<ApplicationUser, long>> logger) : base(dbContext, configuration, userManager, validator, logger)
        {
        }

        protected override async Task<IQueryable<ApplicationUser>> ApplyIdFilter(IQueryable<ApplicationUser> queryable, long id)
        {
            return queryable.Where(x => x.Id == id);
        }

        protected override async Task<IQueryable<ApplicationUser>> ApplyReadSecurity(ApplicationUser applicationUser, IQueryable<ApplicationUser> queryable)
        {
            // This overrides the base implementation of ApplyReadSecurity to allow users to read their own user

            // Site admins can read all users
            if (await _userManager.IsInRoleAsync(applicationUser, ApplicationRoleNames.SuperAdmin))
                return queryable;

            return queryable.Where(x => x.Id == applicationUser.Id);
        }

        protected override async Task<bool> CanWrite(ApplicationUser applicationUser, ApplicationUser dataModel, Dictionary<string, object> extraData)
        {
            if (await _userManager.IsInRoleAsync(applicationUser, ApplicationRoleNames.SuperAdmin))
                return true;

            if (applicationUser.Id == dataModel.Id)
                return true;

            return false;
        }

        protected override async Task<bool> CanInclude(ApplicationUser applicationUser, string include)
        {
            // Site admins can read all users
            if (await _userManager.IsInRoleAsync(applicationUser, ApplicationRoleNames.SuperAdmin))
                return true;

            return false;
        }
    }
}
