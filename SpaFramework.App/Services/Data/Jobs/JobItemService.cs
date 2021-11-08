using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpaFramework.App.DAL;
using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.App.Models.Data.Jobs;
using SpaFramework.Core.Models;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Services.Data.Jobs
{
    public class JobItemService : EntityWriteService<JobItem, long>
    {
        private readonly IClock _clock;

        public JobItemService(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, IValidator<JobItem> validator, ILogger<EntityWriteService<JobItem, long>> logger, IClock clock) : base(dbContext, configuration, userManager, validator, logger)
        {
            _clock = clock;
        }

        protected override async Task<IQueryable<JobItem>> ApplyIdFilter(IQueryable<JobItem> queryable, long id)
        {
            return queryable.Where(x => x.Id == id);
        }

        protected override async Task<IQueryable<JobItem>> ApplyReadSecurity(ApplicationUser applicationUser, IQueryable<JobItem> queryable)
        {
            // Site admins can read all users
            if (await _userManager.IsInRoleAsync(applicationUser, ApplicationRoleNames.SuperAdmin))
                return queryable;

            return queryable.Where(x => false);
        }

        protected override async Task<bool> CanWrite(ApplicationUser applicationUser, JobItem dataModel, Dictionary<string, object> extraData)
        {
            if (await _userManager.IsInRoleAsync(applicationUser, ApplicationRoleNames.SuperAdmin))
                return true;

            return false;
        }
    }
}
