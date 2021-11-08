using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.DAL;
using SpaFramework.Web.Filters.Support;
using SpaFramework.Web.Middleware.Exceptions;
using SpaFramework.App.Models.Data;
using SpaFramework.App;
using Swashbuckle.AspNetCore.Annotations;
using SpaFramework.App.Services.Data;
using SpaFramework.DTO;

namespace SpaFramework.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class EntityReadController<TDataModel, TDTOModel, TService, TIdType> : EntityListController<TDataModel, TDTOModel, TService>
        where TDataModel : class, IEntity, IHasId<TIdType>
        where TDTOModel : class, IDTO
        where TService : IEntityReadService<TDataModel, TIdType>
    {
        public EntityReadController(IConfiguration configuration, IMapper mapper, TService readService) 
            : base(configuration, mapper, readService)
        {
        }

        /// <summary>
        /// Returns a single item
        /// </summary>
        /// <param name="id">The ID of the item to return</param>
        /// <returns></returns>
        [HttpGet("{*id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<TDTOModel>> GetOne(TIdType id, string includes = null)
        {
            id = GetOneId(id);

            TDataModel dataModel = await _readService.GetOne(HttpContext.User, id, includes);

            if (dataModel == null)
                return NotFound();

            TDTOModel dtoModel = MapToDTO(dataModel);

            return Ok(dtoModel);
        }

        protected virtual TIdType GetOneId(TIdType id) { return id; }
    }
}