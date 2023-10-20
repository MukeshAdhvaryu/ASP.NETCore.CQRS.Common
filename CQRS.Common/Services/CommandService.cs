/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/
using CQRS.Common.Contexts;
using CQRS.Common.Interfaces;
using CQRS.Common.Models;

namespace CQRS.Common.Services
{
    #region CommandService<TOutDTO, TModel, TID, TContext>
    /// <summary>
    /// This interface represents repository object to be used in controller class.
    /// </summary>
    /// <typeparam name="TOutDTO">Interface representing the model.</typeparam>
    /// <typeparam name="TModel">Model of your choice.</typeparam>
    /// <typeparam name="TID">Primary key type of the model.</typeparam>
    /// <typeparam name="TContext">Instance which implements IModelContext.</typeparam>
    public partial class CommandService<TOutDTO, TModel, TID, TInDTO, TContext> : Contract, ICommandContract<TOutDTO, TModel, TID>  
        #region TYPE CONSTRINTS
        where TOutDTO : IModel, new()
        where TModel : class, ISelfModel<TID, TModel>,
        //-:cnd:noEmit
#if (!MODEL_USEDTO)
        TOutDTO,
#endif
        //+:cnd:noEmit
        new()
        where TID : struct
        where TInDTO : IModel, new()
        where TContext : IModelContext
        #endregion
    {
        #region VARIABLES
        //-:cnd:noEmit
#if (MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE)
        readonly IExCommand<TOutDTO, TModel, TID> Command;
#endif
        //+:cnd:noEmit
        #endregion

        #region CONSTRUCTORS
        public CommandService(TContext _context, ICollection<TModel>? source = null):
            base(typeof(TModel))
        {
            //-:cnd:noEmit
#if (MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE)
            Command = (IExCommand<TOutDTO, TModel, TID>)_context.CreateCommand<TOutDTO, TModel, TID>(source: source);
#endif
            //+:cnd:noEmit

        }

        //-:cnd:noEmit
#if (MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE)
        public CommandService(ICommand<TOutDTO, TModel, TID> command) :
            base(typeof(TModel))
        {
            Command = (IExCommand<TOutDTO, TModel, TID>)command;
        }
#endif
        //+:cnd:noEmit
        #endregion

        #region PROPERTIES
        //-:cnd:noEmit
#if (MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE)
        ICommand<TOutDTO, TModel, TID> ICommandContract<TOutDTO, TModel, TID>.Command => Command;
#endif
        //+:cnd:noEmit
        public sealed override ContractKind Kind => ContractKind.Cmd;
        #endregion

        #region GET MODEL COUNT
        public override int GetModelCount()
        {
            //-:cnd:noEmit
#if (MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE)
            return  Command.GetModelCount();
#else
            return 0;
#endif
            //+:cnd:noEmit

        }
        #endregion

        #region GET FIRST MODEL
        public TModel? GetFirstModel()
        {
            //-:cnd:noEmit
#if (MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE)
            return  Command.GetFirstModel();
#else
            return default(TModel?);
#endif
            //+:cnd:noEmit

        }
        IModel? IFirstModel.GetFirstModel() =>
            GetFirstModel();
        #endregion
    }
#endregion
}
