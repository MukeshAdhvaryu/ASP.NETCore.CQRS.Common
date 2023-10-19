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
                bool ok = await ((IExCopyable)result).CopyFrom(model);
                if (!ok)
                    throw DummyModel.GetModelException(ExceptionType.ModelCopyOperationFailed, model.ToString());
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

            bool ok = await ((IExCopyable)result).CopyFrom(model);
            if (!ok)
                throw DummyModel.GetModelException(ExceptionType.ModelCopyOperationFailed, model.ToString());
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
