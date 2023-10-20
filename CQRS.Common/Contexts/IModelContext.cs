/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/
//-:cnd:noEmit
#if (MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE) ||(!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)

using CQRS.Common.Models;
#endif
//+:cnd:noEmit

namespace CQRS.Common.Contexts
{
    #region IModelContext
    public interface IModelContext : IDisposable
    {
        #region CREATE COMMAND
        //-:cnd:noEmit
#if (MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE)

        /// <summary>
        /// <summary>
        /// Creates a new instance implementing ICommand<TOutDTO, TModel, TID> interface.
        /// </summary>
        /// <typeparam name="TOutDTO">DTO interface of your choice as a return type of GET calls - must derived from IModel interface.</typeparam>
        /// <typeparam name="TModel">Model implementation of your choice - must derived from Model class.</typeparam>
        /// <typeparam name="TID">Type of primary key such as type of int or Guid etc. </typeparam>
        /// <param name="initialzeData">If true then seed data is added to the internal collection the instance represents.</param>
        /// <param name="source">Optional source - providing pre-existing model data.</param>
        /// <returns>An Instance implementing ICommand<TOutDTO, TModel, TID></returns>
        ICommand<TOutDTO, TModel, TID> CreateCommand<TOutDTO, TModel, TID>(bool initialzeData = true, ICollection<TModel>? source = null)
            #region TYPE CONSTRINTS
            where TOutDTO : IModel, new()
            where TModel : class, ISelfModel<TID, TModel>
        //-:cnd:noEmit
#if (!MODEL_USEDTO)
            , TOutDTO
#endif
           //+:cnd:noEmit
           , new()
            where TID : struct
            #endregion
            ;
#endif
        //+:cnd:noEmit
        #endregion

        #region CREATE QUERY
        //-:cnd:noEmit
#if !MODEL_NONREADABLE || !MODEL_NONQUERYABLE
        /// <summary>
        /// Creates a new instance implementing IQuery<TOutDTO, TModel> interface.
        /// </summary>
        /// <typeparam name="TOutDTO">DTO interface of your choice as a return type of GET calls - must derived from IModel interface.</typeparam>
        /// <typeparam name="TModel">Model implementation of your choice - must derived from Model class.</typeparam>
        /// <param name="initialzeData">If true then seed data is added to the internal collection the instance represents.</param>
        /// <param name="source">Optional source - providing pre-existing model data.</param>
        /// <returns>An Instance implementing IQuery<TOutDTO, TModel></returns>
        IQuery<TOutDTO, TModel> CreateQuery<TOutDTO, TModel>(bool initialzeData = false, ICollection<TModel>? source = null)
            #region TYPE CONSTRAINTS
            where TModel : class, ISelfModel<TModel>
            //-:cnd:noEmit
#if (!MODEL_USEDTO)
            , TOutDTO
#endif
//+:cnd:noEmit
            , new()
            where TOutDTO : IModel, new()
            #endregion
            ;

        /// <summary>
        /// Creates a new instance implementing IQuery<TOutDTO, TModel, TID> interface.
        /// </summary>
        /// <typeparam name="TOutDTO">DTO interface of your choice as a return type of GET calls - must derived from IModel interface.</typeparam>
        /// <typeparam name="TModel">Model implementation of your choice - must derived from Model class.</typeparam>
        /// <typeparam name="TID">Type of primary key such as type of int or Guid etc. </typeparam>
        /// <param name="initialzeData">If true then seed data is added to the internal collection the instance represents.</param>
        /// <param name="source">Optional source - providing pre-existing model data.</param>
        /// <returns>An Instance implementing IQuery<TOutDTO, TModel, TID></returns>
        IQuery<TOutDTO, TModel, TID> CreateQuery<TOutDTO, TModel, TID>(bool initialzeData = false, ICollection<TModel>? source = null)
            #region TYPE CONSTRINTS
            where TOutDTO : IModel, new()
            where TModel : class, ISelfModel<TID, TModel> 
            //-:cnd:noEmit
#if (!MODEL_USEDTO)
            , TOutDTO
#endif
           //+:cnd:noEmit
           , new() 
            where TID : struct
            #endregion
            ;
#endif
        //+:cnd:noEmit
        #endregion
    }
    #endregion
}
