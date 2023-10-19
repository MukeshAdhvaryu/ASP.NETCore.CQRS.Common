/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/
//-:cnd:noEmit
#if !TDD  
//+:cnd:noEmit

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
            await Query.GetAll(0));

            app.MapGet(GetUrl("GetPortion/{count}"), [Tags("Query")] async (int count) => 
                await Query.GetAll(count));

            app.MapGet(GetUrl("GetPortion/{startIndex}, {count}"), [Tags("Query")] async (int startIndex, int count) => 
                await Query.GetAll(startIndex, count));
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
