/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/

using CQRS.Common.Exceptions;
using CQRS.Common.Models;
//-:cnd:noEmit
#if MODEL_SEARCHABLE
using CQRS.Common.Parameters;
#endif
//+:cnd:noEmit

namespace CQRS.Common.Interfaces
{
    #region IModelCount
    /// <summary>
    /// Represents an object which offers a number of models it holds.
    /// </summary>
    public interface IModelCount
    {
        /// <summary>
        /// Gets the number of models this object currently holds.
        /// </summary>
        /// <returns></returns>
        int GetModelCount();
    }
    #endregion

    #region IModifiable
    //-:cnd:noEmit
#if MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE
    /// <summary>
    /// This interface represents an object that is aware of changes in its internal state and allows saving those changes.
    /// </summary>
    public interface IModifiable
    {
        /// <summary>
        /// Saves changes made to this object.
        /// </summary>
        /// <returns>True if the operation is successful; otherwise, false.</returns>
        Task<bool> SaveChanges();
    }
#endif
    //+:cnd:noEmit
    #endregion

    #region IExCopyable
    /// <summary>
    /// This interface represents an object that copies data from another model.
    /// </summary>
    internal interface IExCopyable
    {
        /// <summary>
        /// Copies model data from the given model parameter.
        /// </summary>
        /// <param name="model">Model to copy data from.</param>
        /// <returns>A tuple containing result in terms of sucess and a message detailed one if the operation failed or short one if successful.</returns>
        Task<Tuple<bool, string>> CopyFrom(IModel model);
    }
    #endregion

    #region IExParamParser
    /// <summary>
    /// This interface represents an object that offers parameter parsing capability.
    /// Provided, the given property exist as one of its members.
    /// </summary>
    internal partial interface IExParamParser
    {
        /// <summary>
        /// Parses the specified parameter and if possible emits the value compitible with
        /// the property this object posseses.
        /// </summary>
        /// <param name="propertyName">Name of the property which to parse the value against.</param>
        /// <param name="propertyValue">Value to be parsed to obtain compitible value.</param>
        /// <param name="parsedValue">If succesful, a compitible value parsed using supplied value from parameter.</param>
        /// <param name="updateValueIfParsed">If succesful, replaces the current value with the compitible parsed value.</param>
        /// <returns>Result Message with Status of the parse operation.</returns>
        /// <param name="criteria">Criteria to be used when parsing value.</param>
        bool Parse(string? propertyName, object? propertyValue, out object? parsedValue, bool updateValueIfParsed = false, Criteria criteria = 0);
    }
    #endregion

    #region IExModelToDTO
    //-:cnd:noEmit
#if MODEL_USEDTO
    /// <summary>
    /// This interface represents an object which offers a DTO conversion of itself.
    /// </summary>
    internal interface IExModelToDTO 
    {
        /// <summary>
        /// Provides compitible DTO of given type from this model.
        /// You must implement this method to support dtos.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Compitible DTO.</returns>
        IModel? ToDTO(Type type);
    }
#endif
    //+:cnd:noEmit
    #endregion

    #region IExModelExceptionSupplier
    /// <summary>
    /// This interface represents an object which supplies an appropriate exception for a failure in a specified method.
    /// </summary>
    internal interface IExModelExceptionSupplier
    {
        /// <summary>
        /// Supplies an appropriate exception message for a failure in a specified method.
        /// </summary>
        /// <param name="exceptionType">Type of exception to get.</param>
        /// <param name="additionalInfo">Additional information to aid the task of exception supply.</param>
        /// <returns>Exception message.</returns>
        string GetModelExceptionMessage(ExceptionType exceptionType, string? additionalInfo = null);
    }
    #endregion

    #region IFirstModel 
    /// <summary>
    /// Represents an object which offers the first model with in its internal collection.
    /// </summary>
    public interface IFirstModel
    {
        IModel? GetFirstModel();
    }
    #endregion

    #region IFirstModel<TModel> 
    /// <summary>
    /// Represents an object which offers the first model with in its internal collection.
    /// </summary>
    /// <typeparam name="TModel">Model of your choice.</typeparam>
    /// <typeparam name="TID">Primary key type of the model.</typeparam>
    public interface IFirstModel<TModel> : IFirstModel
        where TModel : ISelfModel<TModel>
    {
        new TModel? GetFirstModel();
    }
    #endregion

    #region IFetch<TModel, TOutDTO>
    //-:cnd:noEmit
#if !MODEL_NONREADABLE || !MODEL_NONQUERYABLE
    public interface IFetch<TOutDTO, TModel>
        where TOutDTO : IModel, new()
        where TModel : IModel
    {
        /// <summary>
        /// Gets all models contained in this object.
        /// The count of models returned can be limited by the count parameter.
        /// If the parameter value is zero, then all models are returned.
        /// </summary>
        /// <param name="count">Number to limit the number of models returned.</param>
        /// <returns>IEnumerable of models.</returns>
        Task<IEnumerable<TOutDTO>?> GetAll(int count = 0);

        /// <summary>
        /// Gets all models contained in this object picking from the index specified up to a count determined by count.
        /// The count of models returned can be limited by the count parameter.
        /// If the parameter value is zero, then all models are returned.
        /// </summary>
        /// <param name="startIndex">Start index which to start picking records from.</param>
        /// <param name="count">Number to limit the number of models returned.</param>
        /// <returns>IEnumerable of models.</returns>
        Task<IEnumerable<TOutDTO>?> GetAll(int startIndex, int count);
    }
#endif
    //+:cnd:noEmit
    #endregion

    #region ISearch<TModel, TOutDTO>
    //-:cnd:noEmit
#if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE) && MODEL_SEARCHABLE
    public interface ISearch<TOutDTO, TModel>
        where TOutDTO : IModel, new()
        where TModel : IModel
    {
        /// <summary>
        /// Finds a model based on given paramters.
        /// </summary>
        /// <param name="parameters">Parameters to be used to find the model.</param>
        /// <param name="conditionJoin">Option from AndOr enum to join search conditions.</param>
        /// <returns>Task with result of collection of type TModel.</returns>
        Task<TOutDTO?> Find<T>(AndOr conditionJoin, params T?[]? parameters)
            where T : ISearchParameter;

        /// <summary>
        /// Finds all models matched based on given parameters.
        /// </summary>
        /// <param name="parameters">Parameters to be used to find the model.</param>
        /// <returns>Task with result of collection of type TModel.</returns>
        /// <param name="conditionJoin">Option from AndOr enum to join search conditions.</param>
        /// <returns>Task with result of collection of type TModel.</returns>
        Task<IEnumerable<TOutDTO>?> FindAll<T>(AndOr conditionJoin, params T?[]? parameters)
            where T : ISearchParameter;
    }
#endif
    //+:cnd:noEmit
    #endregion

    #region IAppendable<TOutDTO, TModel, TID>
    //-:cnd:noEmit
#if MODEL_APPENDABLE
    /// <summary>
    /// This interface represents an object that has a list of models to which a new model can be appended.
    /// Any object that implements the IModel interface can be provided. This allows DTOs to be used instead of an actual model object.
    /// </summary>
    /// <typeparam name="TOutDTO">Interface representing the model.</typeparam>
    /// <typeparam name="TModel">Model of your choice.</typeparam>
    /// <typeparam name="TID">Primary key type of the model.</typeparam>
    public interface IAppendable<TOutDTO, TModel, TID>
        #region TYPE CONSTRINTS
        where TOutDTO : IModel, new()
        where TModel : ISelfModel<TID, TModel>,
        //-:cnd:noEmit
#if (!MODEL_USEDTO)
        TOutDTO,
#endif
        //+:cnd:noEmit
        new()
        where TID : struct
        #endregion
    {
        /// <summary>
        /// Adds a new model based on the given model.
        /// If the given model is not TModel, then a new appropriate model will be created by copying data from the given model.
        /// </summary>
        /// <param name="model">
        /// Any model that implements the IModel interface and has all or a few data members identical to TModel.
        /// This allows DTOs to be used instead of an actual model object.
        /// </param>
        /// <returns>Model that is added.</returns>
        Task<TOutDTO?> Add(IModel? model);

        //-:cnd:noEmit
#if MODEL_APPENDBULK
        /// <summary>
        /// Adds new models based on an enumerable of models specified.
        /// </summary>
        /// <param name="models">An enumerable of models to add to the model collection.</param>
        /// <returns>Collection of models which are successfully added and a message for those which are not.</returns>
        Task<Tuple<IEnumerable<TOutDTO?>?, string>> AddRange<T>(IEnumerable<T?>? models)
            where T: IModel;
#endif
        //+:cnd:noEmit
    }
#endif
    //+:cnd:noEmit
    #endregion

    #region IUpdatable<TOutDTO, TModel, TID>
    //-:cnd:noEmit
#if MODEL_UPDATABLE
    /// <summary>
    /// This interface represents an object that has a list of models and allows a model with a specified ID to be updated with data from the given model parameter.
    /// </summary>
    /// <typeparam name="TOutDTO">Interface representing the model.</typeparam>
    /// <typeparam name="TModel">Model of your choice.</typeparam>
    /// <typeparam name="TID">Primary key type of the model.</typeparam>
    public interface IUpdatable<TOutDTO, TModel, TID>
        #region TYPE CONSTRINTS
        where TOutDTO : IModel, new()
        where TModel : ISelfModel<TID, TModel>,
        //-:cnd:noEmit
#if (!MODEL_USEDTO)
        TOutDTO,
#endif
        //+:cnd:noEmit
        new()
        where TID : struct
        #endregion
    {
        /// <summary>
        /// Updates a model specified by the given ID with the data of the given model.
        /// </summary>
        /// <param name="id">ID of the model to be updated.</param>
        /// <param name="model">
        /// Any model that implements the IModel interface and has all or a few data members identical to TModel.
        /// This allows DTOs to be used instead of an actual model object.
        /// </param>
        /// <returns></returns>
        Task<TOutDTO?> Update(TID id, IModel? model);

        //-:cnd:noEmit
#if MODEL_UPDATEBULK
        /// <summary>
        /// Updates models based on an enumerable of models specified.
        /// </summary>
        /// <param name="IDs">An enumerable of ID to be used to update models matching those IDs from the model collection.</param>
        /// <param name="models">An enumerable of models to update the model collection.</param>
        /// <returns>Collection of models which are successfully updated and a message for those which are not.</returns>
        Task<Tuple<IEnumerable<TOutDTO>?, string>> UpdateRange<T>(IEnumerable<TID>? IDs, IEnumerable<T?>? models)
            where T: IModel;
#endif
        //+:cnd:noEmit

    }
#endif
    //+:cnd:noEmit
    #endregion

    #region IDeletable<TOutDTO, TModel, TID>
    //-:cnd:noEmit
#if MODEL_DELETABLE
    /// <summary>
    /// This interface represents an object that allows deleting a single model with a specified ID.
    /// </summary>
    /// <typeparam name="TOutDTO">Interface representing the model.</typeparam>
    /// <typeparam name="TModel">Model of your choice.</typeparam>
    /// <typeparam name="TID">Primary key type of the model.</typeparam>
    public interface IDeleteable<TOutDTO, TModel, TID>
        #region TYPE CONSTRINTS
        where TOutDTO : IModel, new()
        where TModel : ISelfModel<TID, TModel>,
        //-:cnd:noEmit
#if (!MODEL_USEDTO)
        TOutDTO,
#endif
        //+:cnd:noEmit
        new()
        where TID : struct
        #endregion
    {
        /// <summary>
        /// Deletes the model with the specified ID.
        /// </summary>
        /// <param name="id">ID of the model to delete.</param>
        /// <returns></returns>
        Task<TOutDTO?> Delete(TID id);

        //-:cnd:noEmit
#if MODEL_DELETEBULK
        /// <summary>
        /// Deletes new models based on an enumerable of IDs specified.
        /// </summary>
        /// <param name="IDs">An enumerable of ID to be used to delete models matching those IDs from the model collection.</param>
        /// <returns>Collection of models which are successfully deleted and a message for those which are not.</returns>
        Task<Tuple<IEnumerable<TOutDTO>?, string>> DeleteRange(IEnumerable<TID>? IDs);
#endif
        //+:cnd:noEmit
    }
#endif
    //+:cnd:noEmit
    #endregion

    #region IFindByID<TOutDTO, TModel, TID>
    //-:cnd:noEmit
#if !MODEL_NONREADABLE || !MODEL_NONQUERYABLE
    /// <summary>
    /// This interface represents an object that allows reading a single model or multiple models.
    /// </summary>
    /// <typeparam name="TOutDTO">Interface representing the model.</typeparam>
    /// <typeparam name="TModel">Model of your choice.</typeparam>
    /// <typeparam name="TID">Primary key type of the model.</typeparam>
    public interface IFindByID<TOutDTO, TModel, TID>
        #region TYPE CONSTRINTS
        where TOutDTO : IModel, new()
        where TModel : ISelfModel<TID, TModel>
        //-:cnd:noEmit
#if (!MODEL_USEDTO)
        , TOutDTO
#endif
        //+:cnd:noEmit
        where TID : struct
        #endregion
    {
        /// <summary>
        /// Finds a model based on given id.
        /// </summary>
        /// <param name="id">ID to be used to find the model.</param>
        /// <returns>Task with result of type TModel.</returns>
        Task<TOutDTO?> Get(TID? id);
    }
#endif
    //+:cnd:noEmit
    #endregion

    #region IAdd<TModel>
    //-:cnd:noEmit
#if MODEL_APPENDABLE
    public interface IAdd<TModel> where TModel : IModel
    {
        /// <summary>
        /// Adds a specified model.
        /// </summary>
        /// <param name="model">Model to add.</param>
        /// <returns>Task with result of type boolean.</returns>
        Task<bool> Add(TModel? model);

        /// <summary>
        /// Adds a range of specified models.
        /// </summary>
        /// <param name="models">Models to add.</param>
        /// <returns>Task with result of type boolean.</returns>
        Task<bool> AddRange(IEnumerable<TModel>? models);
    }
#endif
    //+:cnd:noEmit
    #endregion

    #region IDelete<TModel>
    //-:cnd:noEmit
#if MODEL_DELETABLE
    public interface IDelete<TModel> where TModel : IModel
    {
        /// <summary>
        /// Adds a specified model.
        /// </summary>
        /// <param name="model">Model to delete.</param>
        /// <returns>Task with result of type boolean.</returns>
        Task<bool> Delete(TModel? model);

        /// <summary>
        /// Deletes a range of specified models.
        /// </summary>
        /// <param name="models">Models to delete.</param>
        /// <returns>Task with result of type boolean.</returns>
        Task<bool> DeleteRange(IEnumerable<TModel>? models);
    }
#endif
    //+:cnd:noEmit
    #endregion

    #region IUpdate<TModel>
    //-:cnd:noEmit
#if MODEL_UPDATABLE
    public interface IUpdate<TModel> where TModel : IModel
    {
        /// <summary>
        /// Updates a specified model.
        /// </summary>
        /// <param name="model">Model to update.</param>
        /// <returns>Task with result of type boolean.</returns>
        Task<bool> Update(TModel? model);

        /// <summary>
        /// Updates a range of specified models.
        /// </summary>
        /// <param name="models">Models to update.</param>
        /// <returns>Task with result of type boolean.</returns>
        Task<bool> UpdateRange(IEnumerable<TModel>? models);
    }
#endif
    //+:cnd:noEmit
    #endregion

    #region IParser
    /// <summary>
    /// Represents an object which has an ability to parse a query string to a certain type defined as T.
    /// </summary>
    /// <typeparam name="T">Type of result value.</typeparam>
    public interface ISelfParser<T> 
    {
        /// <summary>
        /// Parse the given query string to an instance of type T.
        /// Returns parsed value.
        /// </summary>
        /// <param name="json">Query to be parsed as T.</param>
        /// <returns>An instance of type T.</returns>
        T? Parse(string json);
    }
    #endregion

    #region IParser
    /// <summary>
    /// Represents an object which surrogates as a model binder.
    /// </summary>
    public interface IParser
    { }
    #endregion
}
