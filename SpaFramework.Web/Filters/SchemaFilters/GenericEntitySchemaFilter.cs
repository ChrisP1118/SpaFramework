using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaFramework.Web.Filters.SchemaFilters
{
    public class GenericEntitySchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context?.MemberInfo?.DeclaringType == null)
                return;

            // If there's an explicit description, don't overwrite it
            if (!string.IsNullOrEmpty(schema.Description))
                return;

            string sourceClass = context.MemberInfo.DeclaringType.FullName;
            string sourceProperty = context.MemberInfo.Name;

            if (!sourceClass.EndsWith("DTO"))
                return;

            if (sourceProperty == "ConcurrencyCheck")
                schema.Description = "A value used to enforce optimistic concurrency. When an entity is updated, it must have the same concurrencyCheck value as when the entity was last loaded.";
            else if (sourceProperty == "Inactive")
                schema.Description = "Whether or not the entity is inactive. In general, an inactive entity is ignored or treated as a soft delete.";
            else if (sourceProperty == "LastModification")
                schema.Description = "Returns the timestamp when the entity was last modified. When updating an entity, this is set automatically.";
        }
    }
}
