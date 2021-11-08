using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SpaFramework.Web.Middleware.Exceptions;
using SpaFramework.App.Models.Data;
using SpaFramework.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaFramework.App.Services.Data;
using SpaFramework.DTO;

namespace SpaFramework.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class EntityWriteController<TDataModel, TDTOModel, TService, TIdType> : EntityReadController<TDataModel, TDTOModel, TService, TIdType>
        where TDataModel : class, IEntity, IHasId<TIdType>
        where TDTOModel : class, IDTO
        where TService : IEntityReadService<TDataModel, TIdType>, IEntityWriteService<TDataModel, TIdType>
    {
        protected readonly TService _writeService;

        public EntityWriteController(IConfiguration configuration, IMapper mapper, TService service) : base(configuration, mapper, service)
        {
            _writeService = service;
        }

        /// <summary>
        /// Creates a new item
        /// </summary>
        /// <param name="dtoModel">The item to create</param>
        /// <returns></returns>
        /// <response code="400">Item validation failed</response>
        /// <response code="403">User does not have permission to create item</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationErrorDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TDTOModel>> Create([FromBody] TDTOModel dtoModel)
        {
            TDataModel dataModel = MapFromDTO(dtoModel);

            dataModel = await _writeService.Create(HttpContext.User, dataModel);
            TDTOModel returnValue =  MapToDTO(dataModel);

            return CreatedAtAction(nameof(GetOne), new { id = dataModel.GetId() }, returnValue);
        }

        /// <summary>
        /// Updates an item
        /// </summary>
        /// <param name="id">The ID of the item to update</param>
        /// <param name="dtoModel">The new value for the item</param>
        /// <returns>Returns the item (with an updated timestamp)</returns>
        /// <response code="400">Item validation failed</response>
        /// <response code="403">User does not have permission to update item</response>
        /// <response code="409">Concurrency conflict. The item sent in the request is no longer the most recent version of the item</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update(TIdType id, [FromBody] TDTOModel dtoModel)
        {
            id = GetOneId(id);

            TDataModel dataModel = MapFromDTO(dtoModel);

            dataModel = await _writeService.Update(HttpContext.User, dataModel);

            TDTOModel returnValue = MapToDTO(dataModel);

            return Ok(returnValue);
        }

        /// <summary>
        /// Partially updates an item using JSON patch
        /// </summary>
        /// <param name="id">The ID of the item to update</param>
        /// <param name="patchDocument">The JSON patch operations for the item</param>
        /// <returns>Returns the item (with an updated timestamp)</returns>
        /// <response code="400">Item validation failed</response>
        /// <response code="403">User does not have permission to update item</response>
        /// <response code="404">Item does not exist</response>
        /// <response code="409">Concurrency conflict. The item sent in the request is no longer the most recent version of the item</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Patch(TIdType id, [FromBody] JsonPatchDocument<TDTOModel> patchDocument)
        {
            id = GetOneId(id);

            TDataModel originalDataModel = await _readService.GetOne(HttpContext.User, id, null, true);

            if (originalDataModel == null)
                return NotFound();

            TDTOModel dtoModel = MapToDTO(originalDataModel);
            patchDocument.ApplyTo(dtoModel);
            TDataModel patchedDataModel = MapFromDTO(dtoModel);

            await _writeService.Update(HttpContext.User, patchedDataModel);

            TDTOModel returnValue =  MapToDTO(patchedDataModel);

            return Ok(returnValue);
        }

        /// <summary>
        /// Maps a data transfer object to a data model object. The default implementation uses AutoMapper for the mapping
        /// </summary>
        /// <param name="dtoModel"></param>
        /// <returns></returns>
        protected virtual TDataModel MapFromDTO(TDTOModel dtoModel)
        {
            return _mapper.Map<TDTOModel, TDataModel>(dtoModel);
        }
    }
}
