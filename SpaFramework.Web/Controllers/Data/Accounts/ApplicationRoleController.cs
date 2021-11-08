using AutoMapper;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.DTO.Accounts;
using SpaFramework.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaFramework.App.Services.Data;

namespace SpaFramework.Web.Controllers.Data.Accounts
{
    public class ApplicationRoleController : EntityWriteController<ApplicationRole, ApplicationRoleDTO, IEntityWriteService<ApplicationRole, long>, long>
    {
        public ApplicationRoleController(IConfiguration configuration, IMapper mapper, IEntityWriteService<ApplicationRole, long> service) : base(configuration, mapper, service)
        {
        }
    }
}
