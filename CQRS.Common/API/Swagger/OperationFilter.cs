//-:cnd:noEmit
#if !TDD 
//+:cnd:noEmit

using System.Collections;
using System.Net.Mime;

using CQRS.Common.Interfaces;

using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace CQRS.Common.API
{

    public class OperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            //var type = typeof(IParser);
            //var enumerable = typeof(IEnumerable);
            //var descriptions = context.ApiDescription.ParameterDescriptions;

            //#region GET LIST OF NAMES FOR ALL THE PARAMETERS BOUND BY THE BINDER CLASS.
            //var names = descriptions
            //    .Where
            //    (
            //        ad =>
            //        (ad.Type.IsAssignableTo(type) == true)
            //    ).Select(ad => ad.Name).ToHashSet();
            //#endregion

            //if (names.Count == 0)
            //    return;

            //#region UPDATE ALL PARAMETERS MATCHING OUR LIST TO HAVE JSON CONTENT
            //foreach (var p in operation.Parameters.Where(p => names.Contains(p.Name)))
            //{
            //    if (p.Schema?.Reference == null)
            //    {
            //        var description = descriptions.FirstOrDefault(d => d.Name == p.Name);
            //        if ((description == null))
            //            continue;

            //        p.Content = new Dictionary<string, OpenApiMediaType>()
            //        {
            //            [MediaTypeNames.Application.Json] = new OpenApiMediaType()
            //            {
            //                Schema = GetEnumerableSchema(context, description.Type)
            //            }
            //        };
            //        continue;
            //    }
            //    //Got idea from: https://abdus.dev/posts/aspnetcore-model-binding-json-query-params/
            //    p.Content = new Dictionary<string, OpenApiMediaType>()
            //    {
            //        [MediaTypeNames.Application.Json] = new OpenApiMediaType()
            //        {
            //            Schema = p.Schema
            //        }
            //    };

            //    p.Schema = null;
            //}
            //#endregion
        }

        static OpenApiSchema GetEnumerableSchema(OperationFilterContext context, Type type)
        {
            if (type.IsArray)
                return context.SchemaGenerator.GenerateSchema(type.GetElementType(), context.SchemaRepository);
            return context.SchemaGenerator.GenerateSchema(type.GenericTypeArguments[0], context.SchemaRepository);
        }
    }

}
//-:cnd:noEmit
#endif 
//+:cnd:noEmit

