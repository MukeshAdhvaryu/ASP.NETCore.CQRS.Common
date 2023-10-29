/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/
//-:cnd:noEmit
#if !TDD  
//+:cnd:noEmit
using CQRS.Common.Parsers;

using Microsoft.AspNetCore.Mvc;

namespace CQRS.Common.Services
{
    partial class CommandService<TOutDTO, TModel, TID, TInDTO, TContext> : IExContract
    {
        protected override void Configure(IEndpointRouteBuilder app)
        {
            #region ADD
            //-:cnd:noEmit
#if MODEL_APPENDABLE
            app.MapPost(GetUrl("Add/{model}"), [Tags("Command")] async(Parser<TInDTO?> model) =>
            await Command.Add(model.Result));
#endif
            //+:cnd:noEmit
            #endregion

            #region DELETE
            //-:cnd:noEmit
#if MODEL_DELETABLE
            app.MapDelete(GetUrl("Delete/{id}"), [Tags("Command")] async (TID id) => 
            await Command.Delete(id));

            //+:cnd:noEmit
#endif
            #endregion

            #region UPDATE
            //-:cnd:noEmit
#if MODEL_UPDATABLE
            app.MapPut(GetUrl("Update/{id}/{model}"), [Tags("Command")] async (TID id, Parser<TInDTO?> model) =>
            await Command.Update(id, model.Result));
#endif
            //+:cnd:noEmit
            #endregion

            #region ADD BULK
            //-:cnd:noEmit
#if (MODEL_APPENDABLE) && MODEL_APPENDBULK
            app.MapPost(GetUrl("AddBulk/{models}"), [Tags("Command")] async(Parser<IEnumerable<TInDTO?>?> models) =>
            await Command.AddRange(models.Result));
#endif
            //+:cnd:noEmit
            #endregion

            #region UPDATE BULK
            //-:cnd:noEmit
#if (MODEL_UPDATABLE) && MODEL_UPDATEBULK
            app.MapPut(GetUrl("UpdateBulk/{ids}/{models}"), [Tags("Command")] async (Parser<IEnumerable<TID>> ids, Parser<IEnumerable<TInDTO?>?> models) =>
            await Command.UpdateRange(ids.Result, models.Result));
#endif
            //+:cnd:noEmit
            #endregion

            #region DELETE BULK
            //-:cnd:noEmit
#if (MODEL_DELETABLE) && MODEL_DELETEBULK
            app.MapDelete(GetUrl("DeleteBulk/{ids}"), [Tags("Command")] async (Parser<IEnumerable<TID>> ids) =>
            await Command.DeleteRange(ids.Result));
#endif
            //+:cnd:noEmit
            #endregion

        }
    }
}
//-:cnd:noEmit
#endif
//+:cnd:noEmit
