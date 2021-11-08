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
    public class ClientController : EntityWriteController<Client, ClientDTO, IEntityWriteService<Client, long>, long>
    {
        public ClientController(IConfiguration configuration, IMapper mapper, IEntityWriteService<Client, long> service) : base(configuration, mapper, service)
        {
        }
    }
}
