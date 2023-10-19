/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/

using CQRS.Common.Models;
using CQRS.Common.Interfaces;

namespace CQRS.Common.Contexts
{
    #region ModelContext
    /// <summary>
    /// Represents an object which creates list based model context useful for TDD.
    /// </summary>
    public partial class ModelContext : IModelContext
    {
        #region CREATE COMMAND
        //-:cnd:noEmit
#if MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE
        ICommand<TOutDTO, TModel, TID> IModelContext.CreateCommand<TOutDTO, TModel, TID>(bool initialzeData, ICollection<TModel>? source)
        {
            return newCommandObject<TOutDTO, TModel, TID>(initialzeData, source);
        }

        protected virtual ICommand<TOutDTO, TModel, TID> newCommandObject<TOutDTO, TModel, TID>(bool initialzeData, ICollection<TModel>? source)
            #region TYPE CONSTRINTS
            where TOutDTO :  class, IModel, new()
            where TModel : class, ISelfModel<TID, TModel> 
            //-:cnd:noEmit
#if (!MODEL_USEDTO)
            , TOutDTO
#endif
            //+:cnd:noEmit
            , new()
            where TID : struct
            #endregion
        {
            return new CommandObject<TOutDTO, TModel, TID>(initialzeData, source);
        }
#endif
        //+:cnd:noEmit
        #endregion

        #region CREATE QUERY
        //-:cnd:noEmit
#if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
        IQuery<TOutDTO, TModel> IModelContext.CreateQuery<TOutDTO, TModel>(bool initialzeData, ICollection<TModel>? source)
        {
            return newQueryObject<TOutDTO, TModel>(initialzeData, source);
        }
        protected virtual IQuery<TOutDTO, TModel> newQueryObject<TOutDTO, TModel>(bool initialzeData, ICollection<TModel>? source)
            #region TYPE CONSTRINTS
            where TOutDTO : class, IModel, new()
            where TModel : class, ISelfModel<TModel> 
            //-:cnd:noEmit
#if (!MODEL_USEDTO)
                , TOutDTO
#endif
            //+:cnd:noEmit
            , new()
            #endregion
        {
            return new QueryObject<TOutDTO, TModel>(initialzeData, source);
        }

        IQuery<TOutDTO, TModel, TID> IModelContext.CreateQuery<TOutDTO, TModel, TID>(bool initialzeData, ICollection<TModel>? source)
        {
            return newQueryObject<TOutDTO, TModel, TID>(initialzeData, source);
        }

        protected virtual IQuery<TOutDTO, TModel, TID> newQueryObject<TOutDTO, TModel, TID>(bool initialzeData, ICollection<TModel>? source)
            #region TYPE CONSTRINTS
            where TOutDTO : class, IModel, new()
            where TModel : class, ISelfModel<TID, TModel> 
            //-:cnd:noEmit
#if (!MODEL_USEDTO)
                , TOutDTO
#endif
            //+:cnd:noEmit
            , new()
            where TID : struct
            #endregion
        {
            return new QueryObject<TOutDTO, TModel, TID>(initialzeData, source);
        }

#endif
        //+:cnd:noEmit
        #endregion

        #region DISPOSE
        public void Dispose() { }
        #endregion
    }
    #endregion

    #region ModelContext
    partial class ModelContext
    {
        #region COMMAND CLASS IMPLEMENTATION
        //-:cnd:noEmit
#if MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE
        class CommandObject<TOutDTO, TModel, TID> : Command<TOutDTO, TModel, TID>
            #region TYPE CONSTRINTS
            where TOutDTO : class, IModel, new()
            where TModel : class, ISelfModel<TID, TModel>
            //-:cnd:noEmit
#if (!MODEL_USEDTO)
            , TOutDTO
#endif
            //+:cnd:noEmit
            , new()
            where TID : struct
            #endregion
        {
            ICollection<TModel> models;

            #region CONSTRUCTORS
            public CommandObject(bool initializeData = true, ICollection<TModel>? _models = null)
            {
                models = _models?? new List<TModel>();
                if (!initializeData)
                    return;

                IEnumerable<TModel>? items = GetInitialData();
                if (items == null)
                    return;
                
                if(models is List<TModel>)
                {
                    ((List<TModel>)models).AddRange(items);
                    return;
                }
                foreach (TModel model in items)
                    models.Add(model);
            }
            #endregion

            #region ADD
            //-:cnd:noEmit
#if (MODEL_APPENDABLE)
            protected override void AddModel(TModel model)
            {
                models.Add(model);
            }
#endif
            //+:cnd:noEmit
            #endregion

            #region DELETE
            //-:cnd:noEmit
#if (MODEL_DELETABLE)
            protected override bool DeleteModel(TModel model)
            {
                return models.Remove(model);
            }
#endif
            //+:cnd:noEmit
            #endregion

            #region GET MODEL COUNT
            public override int GetModelCount() => models.Count;
            #endregion

            #region GET BY ID
            protected override Task<TModel?> Get(TID id) =>
              Task.FromResult(models.FirstOrDefault(m => Equals(m.ID, id)));
            #endregion

            #region GET FIRST MODEL
            protected override TModel? GetFirstModel() =>
                models.FirstOrDefault();
            #endregion
        }
#endif
        //+:cnd:noEmit
        #endregion

        #region QUERY CLASS IMPLEMENTATION
        //-:cnd:noEmit
#if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
        class QueryObject<TOutDTO, TModel, TID> : Query<TOutDTO, TModel>, IQuery<TOutDTO, TModel, TID>
            #region TYPE CONSTRINTS
            where TOutDTO : class, IModel, new()
            where TModel : class, ISelfModel<TID, TModel>
            //-:cnd:noEmit
#if (!MODEL_USEDTO)
            , TOutDTO
#endif
            //+:cnd:noEmit
            , new()
             where TID : struct
            #endregion
        {
            ICollection<TModel> models;

            #region CONSTRUCTORS
            public QueryObject(bool initialzeData = false, ICollection<TModel>? _models = null)
            {
                models = _models ?? new List<TModel>();
                if (!initialzeData)
                    return;
                IEnumerable<TModel>? items = GetInitialData();
                if (items == null)
                    return;
                if (models is List<TModel>)
                {
                    ((List<TModel>)models).AddRange(items);
                    return;
                }
                foreach (TModel model in items)
                    models.Add(model);
            }
            #endregion

            #region ITEMS
            protected override IEnumerable<TModel> GetItems() => models;
            #endregion

            #region GET MODEL COUNT
            public override int GetModelCount() => models.Count;
            #endregion

            #region GET MODEL BY ID
            Task<TOutDTO?> IFindByID<TOutDTO, TModel, TID>.Get(TID? id)
            {
                if (GetModelCount() == 0)
                    throw GetModelException(ExceptionType.NoModelsFound);

                TModel? result = models.FirstOrDefault(m => Equals(m.ID, id));

                if (result == null)
                    throw GetModelException(ExceptionType.NoModelFoundForID, id.ToString());
                return Task.FromResult(ToDTO(result));
            }
            #endregion
        }

        class QueryObject<TOutDTO, TModel> : Query<TOutDTO, TModel>
            #region TYPE CONSTRINTS
    where TOutDTO : class, IModel, new()
    where TModel : class, ISelfModel<TModel>
            //-:cnd:noEmit
#if (!MODEL_USEDTO)
    , TOutDTO
#endif
            //+:cnd:noEmit
           , new()
            #endregion
        {
            ICollection<TModel> models;

            #region CONSTRUCTORS
            public QueryObject(bool initialzeData = false, ICollection<TModel>? _models = null)
            {
                models = _models ?? new List<TModel>();
                if (!initialzeData)
                    return;
                IEnumerable<TModel>? items = GetInitialData();
                if (items == null)
                    return;
                if (models is List<TModel>)
                {
                    ((List<TModel>)models).AddRange(items);
                    return;
                }
                foreach (TModel model in items)
                    models.Add(model);
            }
            #endregion

            #region ITEMS
            protected override IEnumerable<TModel> GetItems() => models;
            #endregion

            #region GET MODEL COUNT
            public override int GetModelCount() => models.Count;
            #endregion
        }
#endif
        //+:cnd:noEmit
        #endregion
    }
    #endregion
}
