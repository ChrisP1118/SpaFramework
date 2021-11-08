using FluentValidation;
using SpaFramework.App.DAL;
using SpaFramework.App.Models.Data.Accounts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SpaFramework.App.Models.Data.Donors;
using NodaTime;
using SpaFramework.App.Models;
using System.Collections.Generic;
using SpaFramework.Core.Models;
using SpaFramework.App.Services.WorkItems;
using AutoMapper;
using SpaFramework.App.Models.Data.Clients;

namespace SpaFramework.App.Services.Data.Clients
{
    public class ClientStatsService : EntityReadService<ClientStats, long>
    {
        public ClientStatsService(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, ILogger<ClientStatsService> logger) : base(dbContext, configuration, userManager, logger)
        {
        }

        protected override async Task<IQueryable<ClientStats>> ApplyIdFilter(IQueryable<ClientStats> queryable, long id)
        {
            return queryable.Where(x => x.ClientId == id);
        }

        protected override List<string> ReadRoles => new List<string> { ApplicationRoleNames.SuperAdmin, ApplicationRoleNames.ProjectManager, ApplicationRoleNames.ProjectViewer };
    }
}
