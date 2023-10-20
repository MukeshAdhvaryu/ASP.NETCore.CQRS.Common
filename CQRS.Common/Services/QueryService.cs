/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/

using CQRS.Common.Contexts;
using CQRS.Common.Interfaces;
using CQRS.Common.Models;

namespace CQRS.Common.Services
{
    #region QueryService<TOutDTO, TModel, TContext>
    /// <summary>
    /// This interface represents repository object to be used in controller class.
    /// </summary>
    /// <typeparam name="TOutDTO">Interface representing the model.</typeparam>
    /// <typeparam name="TModel">Model of your choice.</typeparam>
    /// <typeparam name="TContext">Instance which implements IModelContext.</typeparam>
    public partial class QueryService<TOutDTO, TModel, TContext> : Contract, IQueryContract<TOutDTO, TModel>
        #region TYPE CONSTRINTS
        where TOutDTO : IModel, new()
        where TModel : class, ISelfModel<TModel>,
        //-:cnd:noEmit
#if (!MODEL_USEDTO)
        TOutDTO,
#endif
        //+:cnd:noEmit
        new()
        where TContext : IModelContext
        #endregion
    {
        #region VARIABLES
        //-:cnd:noEmit
#if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
        readonly IQuery<TOutDTO, TModel> Query;
#endif
        //-:cnd:noEmit
        #endregion

        #region CONSTRUCTORS
        public QueryService(TContext _context, ICollection<TModel>? source = null) :
            base(typeof(TModel))
        {
            //-:cnd:noEmit
#if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
            CreateQueryObject(out Query, _context, source);
#endif
            //-:cnd:noEmit

        }
        //-:cnd:noEmit
#if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
        public QueryService(IQuery<TOutDTO, TModel> query) :
            base(typeof(TModel))
        {
            Query = query;
        }
#endif
        //+:cnd:noEmit

        #endregion

        #region PROPERTIES
        //-:cnd:noEmit
#if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
        IQuery<TOutDTO, TModel> IQueryContract<TOutDTO, TModel>.Query => Query;
#endif
        //-:cnd:noEmit
        public sealed override ContractKind Kind => ContractKind.Qry;
        #endregion

        #region CREATE QUERY OBJECT
        //-:cnd:noEmit
#if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
        protected virtual void CreateQueryObject(out IQuery<TOutDTO, TModel> query, TContext _context, ICollection<TModel>? source = null)
        {
            query = _context.CreateQuery<TOutDTO, TModel>(true, source);
        }
#endif
        //+:cnd:noEmit
        #endregion

        #region GET MODEL COUNT
        public override int GetModelCount()
        {
            //-:cnd:noEmit
#if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
            return  Query.GetModelCount();
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
#if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
            return  Query.GetFirstModel();
#else
            return default(TModel?);
#endif
            //+:cnd:noEmit

        }
        IModel? IFirstModel.GetFirstModel() =>
            GetFirstModel();
        #endregion

    }
    //+:cnd:noEmit
    #endregion

    #region QueryService<TOutDTO, TModel, TID, TContext>
    /// <summary>
    /// This interface represents repository object to be used in controller class.
    /// </summary>
    /// <typeparam name="TOutDTO">Interface representing the model.</typeparam>
    /// <typeparam name="TModel">Model of your choice.</typeparam>
    /// <typeparam name="TContext">Instance which implements IModelContext.</typeparam>
    public partial class QueryService<TOutDTO, TModel, TID, TContext> :
        QueryService<TOutDTO, TModel, TContext>, IQueryContract<TOutDTO, TModel, TID> 
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
        where TContext : IModelContext
        #endregion
    {
        #region VARIABLES
        //-:cnd:noEmit
#if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
        readonly IQuery<TOutDTO, TModel, TID> Query;
#endif
        //-:cnd:noEmit
        #endregion

        #region CONSTRUCTORS
        public QueryService(TContext _context, ICollection<TModel>? source = null) :
            base(_context, source)
        {
            //-:cnd:noEmit
#if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
            Query = (IQuery<TOutDTO, TModel, TID>)((IQueryContract<TOutDTO, TModel>)this).Query;
#endif
            //-:cnd:noEmit
        }

        //-:cnd:noEmit
#if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
        public QueryService(IQuery<TOutDTO, TModel, TID> query) :
            base(query) 
        {
            Query = query;
        }
#endif
        //-:cnd:noEmit
        #endregion

        #region PROPERTIES
        //-:cnd:noEmit
#if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
        IQuery<TOutDTO, TModel, TID> IQueryContract<TOutDTO, TModel, TID>.Query => Query;
#endif
        //-:cnd:noEmit
        #endregion

        #region CREATE QUERY OBJECT
        //-:cnd:noEmit
#if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
        protected override void CreateQueryObject(out IQuery<TOutDTO, TModel> query, TContext _context, ICollection<TModel>? source = null)
        {
            query = _context.CreateQuery<TOutDTO, TModel, TID>(true, source);
        }
#endif
        //+:cnd:noEmit
        #endregion
    }
    //+:cnd:noEmit
    #endregion
}
