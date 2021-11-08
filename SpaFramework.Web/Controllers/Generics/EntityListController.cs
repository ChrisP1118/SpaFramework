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
    public abstract class EntityListController<TDataModel, TDTOModel, TService> : ControllerBase
        where TDataModel : class, IEntity
        where TDTOModel : class, IDTO
        where TService : IEntityListService<TDataModel>
    {
        protected readonly IConfiguration _configuration;
        protected readonly IMapper _mapper;
        protected readonly TService _readService;

        public EntityListController(IConfiguration configuration, IMapper mapper, TService readService)
        {
            _configuration = configuration;
            _mapper = mapper;
            _readService = readService;
        }

        /// <summary>
        /// Returns all items
        /// </summary>
        /// <param name="offset">The first offset in the result set to return</param>
        /// <param name="limit">The maximum number of results to return</param>
        /// <param name="order">The order in which to sort results</param>
        /// <param name="filter">The filters to apply to the results</param>
        /// <param name="maxCount">The highest number that will be returned for X-Total-Count. By default, there's a limit applied and the full limit isn't returned; if you pass -1 you'll get the full number of results available in X-Total-Count</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [SwaggerResponseHeader(StatusCodes.Status200OK, "X-Total-Count", "int", "Returns the total number of available items (not to exceed X-Total-Count-Max)")]
        [SwaggerResponseHeader(StatusCodes.Status200OK, "X-Total-Count-Max", "int", "Returns the highest total number that could be returned. If this equals X-Total-Count, then there are probably more results available than the number returned in X-Total-Count")]
        public async Task<ActionResult<IEnumerable<TDTOModel>>> GetAll(int offset = 0, int limit = 10, string order = null, string includes = null, string filter = null, int maxCount = -1)
        {
            List<TDataModel> dataModelItems = await _readService.GetAll(HttpContext.User, offset, limit, order, includes, filter, true, null);

            List<TDTOModel> dtoModelItems = dataModelItems
                .Select(d => MapToDTO(d))
                .ToList();

            if (maxCount == -1)
                maxCount = offset + limit + (limit * 3);

            long count = await _readService.GetAllCount(HttpContext.User, filter, maxCount);
            Response.Headers.Add("X-Total-Count", count.ToString());
            Response.Headers.Add("X-Total-Count-Max", maxCount.ToString());

            return Ok(dtoModelItems);
        }

        /// <summary>
        /// Maps a data model object to a data transfer object. The default implementation uses AutoMapper for the mapping
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        protected virtual TDTOModel MapToDTO(TDataModel dataModel)
        {
            return _mapper.Map<TDataModel, TDTOModel>(dataModel);
        }
    }
}