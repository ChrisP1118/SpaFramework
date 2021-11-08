using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Models.Data.Clients;
using SpaFramework.App.Services.Data;
using SpaFramework.DTO.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaFramework.Web.Controllers.Data.Clients
{
    [Authorize]
    public class ClientStatsController : EntityReadController<ClientStats, ClientStatsDTO, IEntityReadService<ClientStats, long>, long>
    {
        public ClientStatsController(IConfiguration configuration, IMapper mapper, IEntityWriteService<ClientStats, long> service) : base(configuration, mapper, service)
        {
        }
    }
}
