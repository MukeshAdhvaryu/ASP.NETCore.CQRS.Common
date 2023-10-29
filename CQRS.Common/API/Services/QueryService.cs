/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/
//-:cnd:noEmit
#if !TDD  
//+:cnd:noEmit

using CQRS.Common.Models;
using CQRS.Common.Parameters;
using CQRS.Common.Parsers;

using Microsoft.AspNetCore.Mvc;

namespace CQRS.Common.Services
{
    #region QueryService<TOutDTO, TModel, TContext>
    partial class QueryService<TOutDTO, TModel, TContext> : IExContract
    {
        protected override void Configure(IEndpointRouteBuilder app)
        {
            //-:cnd:noEmit
#if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
            app.MapGet(GetUrl("GetAll"), [Tags("Query")] async () =>
            await Query.GetAll());

            app.MapGet(GetUrl("GetAll/{count}"), [Tags("Query")] async (int count) =>
            await Query.GetAll(count));

            app.MapGet(GetUrl("GetAll/{startIndex}/{count}"), [Tags("Query")] async (int startIndex, int count) =>
            await Query.GetAll(startIndex, count));

#if MODEL_SEARCHABLE
            app.MapGet(GetUrl("Find/{join}/{parameters}"), [Tags("Query")] async (AndOr join, Parser<SearchParameter[]> parameters) =>
            await Query.Find(join , parameters.Result));

            app.MapGet(GetUrl("FindAll/{join}/{parameters}"), [Tags("Query")] async (AndOr join, Parser<SearchParameter[]> parameters) =>
            await Query.FindAll(join, parameters.Result));
#endif
#endif
            //+:cnd:noEmit
        }
    }
    #endregion

    #region QueryService<TOutDTO, TModel, TID, TContext>
    partial class QueryService<TOutDTO, TModel, TID, TContext> : IExContract
    {
        protected override void Configure(IEndpointRouteBuilder app)
        {
            base.Configure(app);

            //-:cnd:noEmit
#if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
            app.MapGet(GetUrl("Get/{id}"), [Tags("Query")] async (TID id) => 
                await Query.Get(id));
#endif
            //+:cnd:noEmit
        }
    }
    #endregion
}
//-:cnd:noEmit
#endif 
//+:cnd:noEmit
