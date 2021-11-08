using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaFramework.DTO.Accounts;
using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.DTO;
using SpaFramework.App.Models.Service.Content;
using SpaFramework.DTO.Content;
using SpaFramework.App.Models.Data.Content;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Models.Data.Jobs;
using SpaFramework.DTO.Jobs;
using SpaFramework.App.Models.Data.Clients;
using SpaFramework.DTO.Clients;

namespace SpaFramework.Web.Mappings
{
    public class DtoMappingProfile : Profile
    {
        public DtoMappingProfile(IConfiguration configuration)
        {
            // Account data mappings
            CreateMap<RegisterDTO, ApplicationUser>();
            CreateMap<ApplicationUser, ApplicationUserDTO>().ReverseMap();
            CreateMap<ApplicationRole, ApplicationRoleDTO>().ReverseMap();
            CreateMap<ApplicationUserRole, ApplicationUserRoleDTO>().ReverseMap();

            // Generic data mappings
            CreateMap<AllowedToken, AllowedTokenDTO>().ReverseMap();
            CreateMap<ContentBlock, ContentBlockDTO>().ReverseMap();

            CreateMap<Client, ClientDTO>().ReverseMap();
            CreateMap<Project, ProjectDTO>().ReverseMap();
            CreateMap<ClientStats, ClientStatsDTO>().ReverseMap();

            // Internal/service mappings
            CreateMap<ContentData, ContentDataDTO>().ReverseMap();

            CreateMap<Job, JobDTO>().ReverseMap();
        }
    }
}
