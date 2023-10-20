/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/

//-:cnd:noEmit
#if (MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE)
//+:cnd:noEmit
using System.Reflection;
using System.Runtime.CompilerServices;

using CQRS.Common.Attributes;
using CQRS.Common.Exceptions;
using CQRS.Common.Interfaces;
using CQRS.Common.Models;

namespace CQRS.Common
{
    #region Command<TOutDTO, TModel, TID>
    /// <summary>
    /// This class represents an object that allows reading a single model or multiple models.
    /// </summary>
    /// <typeparam name="TOutDTO">Interface representing the model.</typeparam>
    /// <typeparam name="TModel">Model of your choice.</typeparam>
    /// <typeparam name="TID">Primary key type of the model.</typeparam>
    public abstract class Command<TOutDTO, TModel, TID> : IExCommand<TOutDTO, TModel, TID>
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
    {
        #region VARIABLES
        readonly static IExModel DummyModel = (IExModel)new TModel();
        //-:cnd:noEmit
#if MODEL_USEDTO
        static readonly Type DTOType = typeof(TOutDTO);
        static readonly bool NeedToUseDTO = !DTOType.IsAssignableFrom(typeof(TModel));
#endif
        //+:cnd:noEmit
        #endregion

        #region GET INITIAL DATA
        protected IEnumerable<TModel>? GetInitialData()
        {
            IEnumerable<TModel>? items = null;
            bool provideSeedData = false;
            var attribute = typeof(TModel).GetCustomAttribute<DBConnectAttribute>();
            if (attribute != null)
                provideSeedData = attribute.ProvideSeedData;
            if (provideSeedData && GetModelCount() == 0)
            {
                var model = (IExModel)new TModel();
                items = model.GetInitialData()?.OfType<TModel>();
            }
            return items;
        }
        #endregion

        #region ADD
        //-:cnd:noEmit
#if (MODEL_APPENDABLE)
        /// <summary>
        /// Adds a specified model.
        /// </summary>
        /// <param name="model">Model to add.</param>
        /// <returns>Task with type of Model as result.</returns>
        /// <exception cref="Exception"></exception>
        public async virtual Task<TOutDTO?> Add(IModel? model)
        {
            if (model == null)
                throw DummyModel.GetModelException(ExceptionType.NoModelSupplied);
            TModel result;
            if (model is TModel)
                result = (TModel)model;
            else
            {
                result = new TModel();
                var ok = await ((IExCopyable)result).CopyFrom(model);
                if (!ok.Item1)
                    throw DummyModel.GetModelException(ExceptionType.ModelCopyOperationFailed, model.ToString() + System.Environment.NewLine + ok.Item2);
            }
            try
            {
                AddModel(result);
                await SaveChanges();
                return ToDTO(result);
            }
            catch (Exception e)
            {
                throw DummyModel.GetModelException(ExceptionType.AddOperationFailed, null, e);

            }
        }
        protected abstract void AddModel(TModel model);
#endif
        //+:cnd:noEmit
        #endregion

        #region UPDATE
        //-:cnd:noEmit
#if (MODEL_UPDATABLE)
        /// <summary>
        /// Updates a specified model.
        /// </summary>
        /// <param name="modelInterface">Model to update.</param>
        /// <returns>Task with type of Model as result.</returns>
        /// <exception cref="Exception"></exception>
        public async virtual Task<TOutDTO?> Update(TID id, IModel? model)
        {
            if (model == null)
                throw DummyModel.GetModelException(ExceptionType.NoModelSupplied, id.ToString());

            var result = await Get(id);
            if (result == null)
                throw DummyModel.GetModelException(ExceptionType.NoModelFoundForID, id.ToString());

            var ok = await ((IExCopyable)result).CopyFrom(model);
            if (!ok.Item1)
                throw DummyModel.GetModelException(ExceptionType.ModelCopyOperationFailed, model.ToString() + System.Environment.NewLine + ok.Item2);
            try
            {
                await SaveChanges();
                return ToDTO(result);
            }
            catch (Exception e)
            {
                throw DummyModel.GetModelException(ExceptionType.UpdateOperationFailed, id.ToString(), e);
            }
            throw DummyModel.GetModelException(ExceptionType.UpdateOperationFailed, id.ToString());
        }
#endif
        //+:cnd:noEmit
        #endregion

        #region DELETE
        //-:cnd:noEmit
#if (MODEL_DELETABLE)
        /// <summary>
        /// Deltes a specified model.
        /// </summary>
        /// <param name="modelInterface">Model to delete.</param>
        /// <returns>Task with type of Model as result.</returns>
        /// <exception cref="Exception"></exception>
        public async virtual Task<TOutDTO?> Delete(TID id)
        {
            var model = await Get(id);
            if (model == null)
                throw DummyModel.GetModelException(ExceptionType.NoModelFoundForID, id.ToString());
            try
            {
                var result = DeleteModel(model);
                if (result)
                {
                    await SaveChanges();
                    return ToDTO(model);
                }
            }
            catch (Exception e)
            {
                throw DummyModel.GetModelException(ExceptionType.DeleteOperationFailed, id.ToString(), e);
            }
            throw DummyModel.GetModelException(ExceptionType.DeleteOperationFailed, id.ToString());
        }

        protected abstract bool DeleteModel(TModel model);
#endif
        //+:cnd:noEmit
        #endregion

        #region ADD RANGE
        //-:cnd:noEmit
#if (MODEL_APPENDABLE) && MODEL_APPENDBULK
        /// <summary>
        /// Adds new models based on an enumerable of models specified.
        /// </summary>
        /// <param name="models">An enumerable of models to add to the model collection.</param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<TOutDTO?>?, string>> AddRange<T>(IEnumerable<T?>? models)
            where T : IModel
        {
            if(models == null)
                throw DummyModel.GetModelException(ExceptionType.NoModelsSupplied);
            
            List<TModel> results = new List<TModel> ();  
            var sb = new List<string?>();
            int i = -1;

            foreach (var model in models)
            {
                ++i;
                if (model == null)
                {
                    sb.Add(DummyModel.GetModelExceptionMessage(ExceptionType.NoModelSupplied));
                    sb.Add("Index: " + i);
                    continue;
                }

                TModel result = (model is TModel)? (TModel)(object)model:  new TModel();
                var ok = await ((IExCopyable)result).CopyFrom(model);
                if (!ok.Item1)
                {
                    sb.Add(DummyModel.GetModelExceptionMessage(ExceptionType.ModelCopyOperationFailed, model.ToString()));
                    sb.Add("Index: " + i + ": " + ok.Item2);
                    continue;
                }
                try
                {
                    AddModel(result);
                    results.Add(result);
                }
                catch (Exception e)
                {
                    var message = DummyModel.GetModelExceptionMessage(ExceptionType.AddOperationFailed, model.ToString());
                    sb.Add("Index: " + i + ": " + ok.Item2);
                    sb.Add(e.Message);
                    if (!Globals.IsProductionEnvironment)
                        sb.Add(e.StackTrace);
                }
            }

            if (results.Count == 0)
            {
                if (sb.Count > 0)
                {
                    var mainMessage = sb[0];
                    throw DummyModel.GetModelException(ExceptionType.NoModelsSupplied, mainMessage,
                        new Exception(string.Join(Environment.NewLine, sb.Skip(1).Where(s => !string.IsNullOrEmpty(s)))));
                }
                throw DummyModel.GetModelException(ExceptionType.NoModelsSupplied, sb.ToString());
            }

            await SaveChanges();
            return Tuple.Create(ToDTO(results), sb.Count > 0? string.Join(Environment.NewLine, 
                sb.Skip(1).Where(s => !string.IsNullOrEmpty(s))): "All Sucess");
        }
#endif
        //+:cnd:noEmit
        #endregion

        #region UPDATE RANGE
        //-:cnd:noEmit
#if (MODEL_UPDATABLE) && MODEL_UPDATEBULK
        /// <summary>
        /// Updates models based on an enumerable of models specified.
        /// </summary>
        /// <param name="IDs">An enumerable of ID to be used to update models matching those IDs from the model collection.</param>
        /// <param name="models">An enumerable of models to update the model collection.</param>
        /// <returns>Collection of models which are successfully updated and a message for those which are not.</returns>
        public async Task<Tuple<IEnumerable<TOutDTO>?, string>> UpdateRange<T>(IEnumerable<TID>? IDs, IEnumerable<T?>? models)
            where T : IModel
        {
            if(models == null)
                throw DummyModel.GetModelException(ExceptionType.NoModelsSupplied, IDs == null? "Even IDs are not supplied!": "IDs are supplied though!");
            
            if(IDs == null)
                throw DummyModel.GetModelException(ExceptionType.NoIDsSupplied, models == null? "Even models are not supplied!": "models are supplied though!");

            List<TModel> results = new List<TModel> ();
            var sb = new List<string?>();

            var idList = IDs.ToArray();
            var modelList = models.ToArray();
            var idCount = idList.Length;
            var modelCount = modelList.Length;

            for (int i = 0; i < idCount; i++)
            {
                if (i >= modelCount)
                {
                    sb.Add(DummyModel.GetModelExceptionMessage(ExceptionType.NoModelSupplied, modelList[i]?.ToString()));
                    sb.Add("For: " + idList[i] + " at Index: " + i);
                    continue;
                }
                var model = modelList[i];
                if (model == null)
                {
                    sb.Add(DummyModel.GetModelExceptionMessage(ExceptionType.NoModelSupplied));
                    sb.Add("For: " + idList[i] + " at Index: " + i);
                    continue;
                }
                var id = idList[i];
                var result = await Get(id);
                if (result == null)
                {
                    sb.Add(DummyModel.GetModelExceptionMessage(ExceptionType.NoModelFoundForID, id.ToString() + " at Index: " + i));
                    continue;
                }

                var ok = await ((IExCopyable)result).CopyFrom(model);
                if (!ok.Item1)
                {
                    sb.Add(DummyModel.GetModelExceptionMessage(ExceptionType.ModelCopyOperationFailed, model.ToString()));
                    sb.Add("For: " + idList[i] + " at Index: " + i);
                    sb.Add(ok.Item2);
                    continue;
                }
                results.Add(result);
            }

            if (results.Count == 0)
            {
                if (sb.Count > 0)
                {
                    var mainMessage = sb[0];
                    throw DummyModel.GetModelException(ExceptionType.NoModelsSupplied, mainMessage,
                        new Exception(string.Join(Environment.NewLine, sb.Skip(1).Where(s => !string.IsNullOrEmpty(s)))));
                }
                throw DummyModel.GetModelException(ExceptionType.NoModelsSupplied, sb.ToString());
            }

            await SaveChanges();
            return Tuple.Create(ToDTO(results), sb.Count > 0 ? string.Join(Environment.NewLine,
                sb.Skip(1).Where(s => !string.IsNullOrEmpty(s))) : "All Sucess");
        }
#endif
        //+:cnd:noEmit
        #endregion

        #region DELETE RANGE
        //-:cnd:noEmit
#if (MODEL_DELETABLE) && MODEL_DELETEBULK
        /// <summary>
        /// Deletes new models based on an enumerable of IDs specified.
        /// </summary>
        /// <param name="IDs">An enumerable of ID to be used to delete models matching those IDs from the model collection.</param>
        /// <returns>Collection of models which are successfully deleted and a message for those which are not.</returns>        
        public async Task<Tuple<IEnumerable<TOutDTO>?, string>> DeleteRange(IEnumerable<TID>? IDs)
        {
            if(IDs == null)
                throw DummyModel.GetModelException(ExceptionType.NoIDsSupplied);

            var results = new List<TModel>();
            var sb = new List<string?>();

            int i = -1;
            foreach (var id in IDs)
            {
                ++i;
                var model = await Get(id);
                if (model == null)
                {
                    sb.Add(DummyModel.GetModelExceptionMessage(ExceptionType.NoModelFoundForID, id.ToString()));
                    continue;
                }
                try
                {
                    var result = DeleteModel(model);
                    if (result)
                    {
                        results.Add(model);
                    }
                }
                catch (Exception e)
                {
                    var message = DummyModel.GetModelExceptionMessage(ExceptionType.DeleteOperationFailed, "For ID: " + id.ToString() + " at Index: " + i );
                    sb.Add(message);
                    sb.Add(e.Message);
                    if (!Globals.IsProductionEnvironment)
                        sb.Add(e.StackTrace);
                }
            }

            if (results.Count == 0)
            {
                if (sb.Count > 0)
                {
                    var mainMessage = sb[0];
                    throw DummyModel.GetModelException(ExceptionType.NoModelsSupplied, mainMessage,
                        new Exception(string.Join(Environment.NewLine, sb.Skip(1).Where(s => !string.IsNullOrEmpty(s)))));
                }
                throw DummyModel.GetModelException(ExceptionType.NoModelsSupplied, sb.ToString());
            }

            await SaveChanges();
            return Tuple.Create(ToDTO(results), sb.Count > 0 ? string.Join(Environment.NewLine,
                sb.Skip(1).Where(s => !string.IsNullOrEmpty(s))) : "All Sucess");
        }
#endif
        //+:cnd:noEmit
        #endregion

        #region GET MODEL COUNT
        public abstract int GetModelCount();
        #endregion

        #region GET FIRST MODEL
        protected abstract TModel? GetFirstModel();
        TModel? IFirstModel<TModel>.GetFirstModel() =>
            GetFirstModel();
        IModel? IFirstModel.GetFirstModel() =>
            GetFirstModel();
        #endregion

        #region GET BY ID
        protected abstract Task<TModel?> Get(TID id);
        #endregion

        #region SAVE CHANGES
        //-:cnd:noEmit
#if (MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE)
        public virtual Task<bool> SaveChanges() =>
            Task.FromResult(true);
#endif
        //+:cnd:noEmit
        #endregion

        #region GET MODEL EXCEPTION
        /// <summary>
        /// Supplies an appropriate exception for a failure in a specified method.
        /// </summary>
        /// <param name="exceptionType">Type of exception to get.</param>
        /// <param name="additionalInfo">Additional information to aid the task of exception supply.</param>
        /// <param name="innerException">Inner exception which is already thrown.</param>
        /// <returns>Instance of SpecialException class.</returns>
        protected ModelException GetModelException(ExceptionType exceptionType, string? additionalInfo = null, Exception? innerException = null) =>
            DummyModel.GetModelException(exceptionType, additionalInfo, innerException);
        #endregion

        #region TO DTO
        /// <summary>
        /// Converts model to an apporiate object of TOutDTO type.
        /// </summary>
        /// <param name="model">Model to convert.</param>
        /// <returns>Converted model ito an apporiate object of TOutDTO type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TOutDTO? ToDTO(TModel? model)
        {
            if (model == null)
                return default(TOutDTO);
            //-:cnd:noEmit
#if (MODEL_USEDTO)
            if (NeedToUseDTO)
            {
                var result = ((IExModel)model).ToDTO(DTOType);
                if (result == null)
                    return default(TOutDTO);
                return (TOutDTO?)((IExModel)model).ToDTO(DTOType);
            }
#endif
            //+:cnd:noEmit
            return (TOutDTO?)(object)model;
        }

        /// <summary>
        /// Converts models to an apporiate objects of TOutDTO type.
        /// </summary>
        /// <param name="model">Models to convert.</param>
        /// <returns>Converted models to an apporiate objects of TOutDTO type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual IEnumerable<TOutDTO>? ToDTO(IEnumerable<TModel>? models)
        {
            if (models == null)
                return Enumerable.Empty<TOutDTO>();
            //-:cnd:noEmit
#if (MODEL_USEDTO)
            if (NeedToUseDTO)
                return (IEnumerable<TOutDTO>?)models.Select(m => ToDTO(m));
#endif
            //+:cnd:noEmit

            return (IEnumerable<TOutDTO>?)models;
        }
        #endregion
    }
    #endregion
}
//-:cnd:noEmit
#endif
//+:cnd:noEmit
