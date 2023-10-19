/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/
//-:cnd:noEmit
#if !TDD  
//+:cnd:noEmit
using Microsoft.AspNetCore.Mvc;

namespace CQRS.Common.Services
{
    partial class CommandService<TOutDTO, TModel, TID, TInDTO, TContext> : IExContract
    {
        protected override void Configure(IEndpointRouteBuilder app)
        {
            //-:cnd:noEmit
#if MODEL_APPENDABLE
            app.MapPut(GetUrl("Add/{model}"), [Tags("Command")] async ([FromBody] TInDTO? model) => await Command.Add(model));
#endif
#if MODEL_DELETABLE
            app.MapDelete(GetUrl("Delete/{id}"), [Tags("Command")] async (TID id) => await Command.Delete(id));
#endif
#if MODEL_UPDATABLE
            app.MapDelete(GetUrl("Update/{id},{model}"), [Tags("Command")] async (TID id, [FromBody] TInDTO? model) => await Command.Update(id, model));
#endif
            //+:cnd:noEmit
        }
    }
}
//-:cnd:noEmit
#endif
//+:cnd:noEmit
