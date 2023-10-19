/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/
//-:cnd:noEmit
#if !TDD  
//+:cnd:noEmit

//-:cnd:noEmit
#if (MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE) ||(!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
#endif
//+:cnd:noEmit
using CQRS.Common.Contexts;
using CQRS.Common.Interfaces;
using CQRS.Common.Models;

using Microsoft.EntityFrameworkCore;

namespace CQRS.Common.Minimal.API
{
    #region DBCONTEXT
    public partial class DBContext : DbContext, IModelContext
    {
        #region CONSTRUCTOR
        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        { }
        #endregion

        #region ON MODEL CREATION
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder = modelBuilder.ApplyConfigurationsFromAssembly(typeof(DBContext).Assembly);
        }
        #endregion

        #region CREATE COMMAND
        //-:cnd:noEmit
#if MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE
        ICommand<TOutDTO, TModel, TID> IModelContext.CreateCommand<TOutDTO, TModel, TID>(bool initialzeData, ICollection<TModel>? source)
        {
        
            return new CommandObject<TOutDTO, TModel, TID>(this, source, initialzeData);
        }
#endif
        //+:cnd:noEmit
        #endregion

        #region CREATE QUERY
        //-:cnd:noEmit
#if !MODEL_NONREADABLE || !MODEL_NONQUERYABLE
        IQuery<TOutDTO, TModel> IModelContext.CreateQuery<TOutDTO, TModel>(bool initialzeData, ICollection<TModel>? source)
        {
            return new QueryObject<TOutDTO, TModel>(this, null, source, initialzeData);
        }
        IQuery<TOutDTO, TModel, TID> IModelContext.CreateQuery<TOutDTO, TModel, TID>(bool initialzeData, ICollection<TModel>? source)
        {
            return new QueryObject<TOutDTO, TModel, TID>(this, null, source, initialzeData);
        }
#endif
        //+:cnd:noEmit
        #endregion
    }
    #endregion

    partial class DBContext
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
            DbSet<TModel> models;
            DBContext context;

            #region CONSTRUCTORS
            public CommandObject(DBContext _context, ICollection<TModel>? source = null, bool initializeData = true)
            {
                context = _context;
                models = context.Set<TModel>();
                if(source != null)
                {
                    try
                    {
                        models.AddRange(source);
                        context.SaveChanges();

                    }
                    catch  
                    {
                        throw;
                    }
                }
                if (!initializeData)
                    return;

                IEnumerable<TModel>? items = GetInitialData();
                if (items == null)
                    return;
                models.AddRange(items);
                context.SaveChanges();
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
                models.Remove(model);
                return true;
            }
#endif
            //+:cnd:noEmit
            #endregion

            #region GET MODEL COUNT
            public override int GetModelCount() => models.Count();
            #endregion

            #region GET FIRST MODEL
            protected override TModel? GetFirstModel() => 
                models.FirstOrDefault();
            #endregion

            #region GET BY ID
            protected override Task<TModel?> Get(TID id) =>
              Task.FromResult(models.FirstOrDefault(m => Equals(m.ID, id)));
            #endregion

            #region SAVE CHANGES
            public override async Task<bool> SaveChanges()
            {
                return await context.SaveChangesAsync() > 0;
            }
            #endregion
        }
#endif
        //+:cnd:noEmit
        #endregion

        #region QUERY CLASS IMPLEMENTATION
        //-:cnd:noEmit
#if !MODEL_NONREADABLE || !MODEL_NONQUERYABLE
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
            DbSet<TModel> models;

            #region CONSTRUCTORS
            public QueryObject(DBContext context, DbSet<TModel>? _models = null, ICollection<TModel>? source = null, bool initialzeData = false)
            {
                models = _models?? context.Set<TModel>();
                if (source != null)
                {
                    try
                    {
                        models.AddRange(source);
                        context.SaveChanges();
                    }
                    catch
                    {
                        throw;
                    }
                }
                if (!initialzeData)
                    return;
                IEnumerable<TModel>? items = GetInitialData();
                if (items == null)
                    return;
                models.AddRange(items);
                context.SaveChanges();
            }
            #endregion

            #region ITEMS
            protected override IEnumerable<TModel> GetItems() => models;
            #endregion

            #region GET MODEL COUNT
            public override int GetModelCount() => models.Count();
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
            DbSet<TModel> models;

            #region CONSTRUCTORS
            public QueryObject(DBContext context, DbSet<TModel>? _models = null, ICollection<TModel>? source = null, bool initialzeData = false)
            {
                models = _models ?? context.Set<TModel>();
                if (source != null)
                {
                    try
                    {
                        models.AddRange(source);
                        context.SaveChanges();
                    }
                    catch
                    {
                        throw;
                    }
                }

                if (!initialzeData)
                    return;
                IEnumerable<TModel>? items = GetInitialData();
                if (items == null)
                    return;
                models.AddRange(items);
                context.SaveChanges();
            }
            #endregion

            #region ITEMS
            protected override IEnumerable<TModel> GetItems() => models;
            #endregion

            #region GET MODEL COUNT
            public override int GetModelCount() => models.Count();
            #endregion
        }
#endif
        //+:cnd:noEmit
        #endregion
    }
}
//-:cnd:noEmit
#endif
//+:cnd:noEmit
