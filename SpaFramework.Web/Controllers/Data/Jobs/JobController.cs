using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Models.Data.Jobs;
using SpaFramework.App.Services.Data;
using SpaFramework.DTO.Jobs;

namespace SpaFramework.Web.Controllers.Data.Jobs
{
    [Authorize]
    public class JobController : EntityWriteController<Job, JobDTO, IEntityWriteService<Job, long>, long>
    {
        public JobController(IConfiguration configuration, IMapper mapper, IEntityWriteService<Job, long> service) : base(configuration, mapper, service)
        {
        }
    }
}
