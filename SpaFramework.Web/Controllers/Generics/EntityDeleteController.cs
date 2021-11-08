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
    public abstract class EntityDeleteController<TDataModel, TDTOModel, TService, TIdType> : EntityWriteController<TDataModel, TDTOModel, TService, TIdType>
        where TDataModel : class, IEntity, IHasId<TIdType>
        where TDTOModel : class, IDTO
        where TService : IEntityDeleteService<TDataModel, TIdType>
    {
        public EntityDeleteController(IConfiguration configuration, IMapper mapper, TService service) : base(configuration, mapper, service)
        {
        }

        /// <summary>
        /// Deletes an item
        /// </summary>
        /// <param name="id">The ID of the item to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(TIdType id)
        {
            id = GetOneId(id);

            await _writeService.Delete(HttpContext.User, id);

            return NoContent();
        }
    }
}
