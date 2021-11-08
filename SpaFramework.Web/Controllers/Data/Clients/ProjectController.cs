using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Models.Data.Donors;
using SpaFramework.App;
using SpaFramework.App.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaFramework.App.Models.Data.Clients;
using SpaFramework.DTO.Clients;

namespace SpaFramework.Web.Controllers.Data.Clients
{
    [Authorize]
    public class ProjectController : EntityWriteController<Project, ProjectDTO, IEntityWriteService<Project, long>, long>
    {
        public ProjectController(IConfiguration configuration, IMapper mapper, IEntityWriteService<Project, long> service) : base(configuration, mapper, service)
        {
        }
    }
}
