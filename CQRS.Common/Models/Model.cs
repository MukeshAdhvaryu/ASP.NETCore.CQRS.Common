/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

using CQRS.Common.Models;
using CQRS.Common.Exceptions;
using CQRS.Common.Interfaces;
using CQRS.Common.Parameters;

namespace CQRS.Common.Models
{
    #region Model
    public abstract partial class Model<TModel> : ISelfModel<TModel>, IExModel
        //-:cnd:noEmit
#if MODEL_SEARCHABLE
        , IMatch
#endif
        //+:cnd:noEmit
        where TModel : Model<TModel>, ISelfModel<TModel>
    {
        #region VARIABLES
        readonly string modelName;
        #endregion

        #region CONSTRUCTOR
        protected Model()
        {
            var type = GetType();
            modelName = type.Name;
        }
        #endregion

        #region PROPERTIES
        public string ModelName => modelName;
        #endregion

        #region GET PROPERTY NAMES
        /// <summary>
        /// Provides a list of names of properties - must be handled while copying from data supplied from model binder's BindModelAsync method.
        /// If the list is not provided, System.Reflecteion will be used to obtain names of the properties defined in this model.
        /// </summary>
        protected virtual IReadOnlyList<string> GetPropertyNames(bool forSearch = false) => null;
        IReadOnlyList<string> IExModel.GetPropertyNames(bool forSearch)
        {
            var propertyNames = GetPropertyNames(forSearch);

            if (propertyNames == null || propertyNames.Count == 0)
            {
                propertyNames = GetType().GetProperties().Where(p =>
                {
                    var attr = p.GetType().GetCustomAttribute<DatabaseGeneratedAttribute>();
                    if (attr?.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed)
                        return false;
                    return true;
                }).Select(p => p.Name).ToArray();
            }
            return propertyNames;
        }
        #endregion

        #region PARSE
        /// <summary>
        /// Parses the specified parameter and if possible emits the value compitible with
        /// the property this object posseses.
        /// </summary>
        /// <param name="parameter">Parameter to parse.</param>
        /// <param name="currentValue">Current value exists for the given property in this object.</param>
        /// <param name="parsedValue">If succesful, a compitible value parsed using supplied value from parameter.</param>
        /// <param name="updateValueIfParsed">If succesful, replace the current value with the compitible parsed value.</param>
        /// <returns>Result Message with Status of the parse operation.</returns>
        protected abstract Message Parse(ObjParameter parameter, out object? currentValue, out object? parsedValue, bool updateValueIfParsed = false);
        Message IExParamParser.Parse(ObjParameter parameter, out object? currentValue, out object? parsedValue, bool updateValueIfParsed, Criteria criteria)
        {
            var name = parameter.Name;
            parsedValue = null;
            object? value;
            
            switch (name)
            {
                default:
                    switch (criteria)
                    {
                        case Criteria.Occurs:
                        case Criteria.BeginsWith:
                        case Criteria.EndsWith:
                        case Criteria.OccursNoCase:
                        case Criteria.BeginsWithNoCase:
                        case Criteria.EndsWithNoCase:
                        case Criteria.StringEqual:
                        case Criteria.StringEqualNoCase:
                        case Criteria.StringNumGreaterThan:
                        case Criteria.StringNumLessThan:
                        case Criteria.NotOccurs:
                        case Criteria.NotBeginsWith:
                        case Criteria.NotEndsWith:
                        case Criteria.NotOccursNoCase:
                        case Criteria.NotBeginsWithNoCase:
                        case Criteria.NotEndsWithNoCase:
                        case Criteria.NotStrEqual:
                        case Criteria.NotStrEqualNoCase:
                        case Criteria.NotStringGreaterThan:
                        case Criteria.NotStringLessThan:
                            value = parameter is ModelParameter ? ((ModelParameter)parameter).FirstValue : parameter.Value;
                            parsedValue = value?.ToString();
                            Parse(parameter, out currentValue, out _, updateValueIfParsed);
                            return Message.Sucess(name);
                        default:
                            break;
                    }
                    break;
            }
            return Parse(parameter, out currentValue, out parsedValue, updateValueIfParsed);
        }
        #endregion

        #region COPY FROM
        /// <summary>
        /// Copies data from the given model to this instance.
        /// </summary>
        /// <param name="model">Model to copy data from.</param>
        /// <returns></returns>
        protected abstract Task<bool> CopyFrom(IModel model);
        Task<bool> IExCopyable.CopyFrom(IModel model) =>
            CopyFrom(model);
        #endregion

        #region GET INITIAL DATA
        /// <summary>
        /// Gets initial data.
        /// </summary>
        /// <returns>IEnumerable\<IModel\> containing list of initial data.</returns>
        protected abstract IEnumerable<IModel> GetInitialData();
        IEnumerable<IModel> IExModel.GetInitialData() =>
            GetInitialData();
        #endregion

        #region IsMATCH
//-:cnd:noEmit
#if MODEL_SEARCHABLE
        /// <summary>
        /// Matches property with specified name using criteria given and emits current value exists for the given property
        /// and a compitible value parsed using supplied value from parameter.
        /// </summary>
        /// <param name="propertyName">Name of property which to match for.</param>
        /// <param name="criteria">Criteria enum to specify on the grounds the match should perform.</param>
        /// <param name="currentValue">Current value exists for the given property.</param>
        /// <param name="parsedValue">If succesful, a compitible value parsed using supplied value from parameter.</param>
        /// <returns>True if values match, otherwise false.</returns>
        protected virtual bool IsMatch(string propertyName, Criteria criteria, object? currentValue, object? parsedValue)
        {
            if (parsedValue == null)
            {
                if (currentValue == null && criteria == Criteria.Equal)
                    return true;
                    
                return false;
            }
            return Operations.Compare(currentValue, criteria, parsedValue);
        }
        bool IMatch.IsMatch(SearchParameter? parameter)
        {
            if(parameter == null) 
                return false; 
            var result = ((IExParamParser)this).Parse(parameter, out var currentValue, out var newValue, false, parameter.Criteria);
            switch (result.Status)
            {
                case ResultStatus.Sucess:
                    return IsMatch(parameter.Name, parameter.Criteria, currentValue, newValue);
                case ResultStatus.Failure:
                case ResultStatus.Ignored:
                case ResultStatus.MissingValue:
                case ResultStatus.MissingRequiredValue:
                default:
                    return false;
            }
        }
                //-:cnd:noEmit
#endif
#endregion

        #region GET APPROPRIATE EXCEPTION
        /// <summary>
        /// Supplies an appropriate exception for a failure in a specified method.
        /// </summary>
        /// <param name="exceptionType">Type of exception to get.</param>
        /// <param name="additionalInfo">Additional information to aid the task of exception supply.</param>
        /// <returns>Instance of SpecialException class.</returns>
        protected virtual ModelException GetAppropriateException(ExceptionType exceptionType, string? additionalInfo = null, Exception? innerException = null)
        {
            bool noAdditionalInfo = string.IsNullOrEmpty(additionalInfo);

            switch (exceptionType)
            {
                case ExceptionType.NoModelFound:
                case ExceptionType.NoModelFoundForID:
                    return ModelException.Create(string.Format("No {0} is found additional info: {1}", modelName, noAdditionalInfo ? "None" : "ID = " + additionalInfo), exceptionType, innerException);

                case ExceptionType.NoModelsFound:
                    return ModelException.Create(string.Format("No {0} are found additional info: {1}", modelName, additionalInfo ?? " None"), exceptionType, innerException);

                case ExceptionType.NoModelSupplied:
                    return ModelException.Create(string.Format("Null {0} can not be supplied additional info: {1}", modelName, additionalInfo ?? " None"), exceptionType, innerException);

                case ExceptionType.NegativeFetchCount:
                    return ModelException.Create(string.Format("{0} fetch count must be > 0; provided: {1}", modelName, additionalInfo ?? " None"), exceptionType, innerException);

                case ExceptionType.ModelCopyOperationFailed:
                    return ModelException.Create(string.Format("Copy operation of {0} is failed; model provided: {1}", modelName, additionalInfo ?? " None"), exceptionType, innerException);

                case ExceptionType.NoParameterSupplied:
                    return ModelException.Create(string.Format("Null parameter for searching a {0} is not allowed; additional info: {1}", modelName, additionalInfo ?? " None"), exceptionType, innerException);

                case ExceptionType.NoParametersSupplied:
                    return ModelException.Create(string.Format("Null parameters for searching  {0}s are not allowed; additional info: {1}", modelName, additionalInfo ?? " None"), exceptionType, innerException);

                case ExceptionType.AddOperationFailed:
                    return ModelException.Create(string.Format("Add operation for adding new {0} is failed; additional info: {1}", modelName, additionalInfo ?? " None"), exceptionType, innerException);

                case ExceptionType.UpdateOperationFailed:
                    return ModelException.Create(string.Format("Update operation for updating the {0} is failed; additional info: {1}", modelName, additionalInfo ?? " None"), exceptionType, innerException);

                case ExceptionType.DeleteOperationFailed:
                    return ModelException.Create(string.Format("Delete operation for deleting the {0} is failed; additional info: {1}", modelName, additionalInfo ?? " None"), exceptionType, innerException);

                case ExceptionType.InternalServerError:
                    return ModelException.Create(string.Format("Model {0}: internal server error; additional info: {1}", modelName, additionalInfo ?? " None"), exceptionType, innerException);

                case ExceptionType.ExpectationFailed:
                    return ModelException.Create(string.Format("Model {0}: expectation failed; additional info: {1}", modelName, additionalInfo ?? " None"), exceptionType, innerException);

                case ExceptionType.InvalidContext:
                    return ModelException.Create(string.Format("The supplied model context is not valid or compitible with the {0}", additionalInfo ?? " None"), exceptionType, innerException);

                default:
                    return ModelException.Create("Need to supply your message", ExceptionType.Unknown);
            }
        }
        ModelException IExModelExceptionSupplier.GetModelException(ExceptionType exceptionType, string? additionalInfo, Exception? innerException) =>
            GetAppropriateException(exceptionType, additionalInfo, innerException);
        #endregion

        #region IModelToDTO
        //-:cnd:noEmit
#if MODEL_USEDTO
        /// <summary>
        /// Provides compitible DTO of given type from this model.
        /// You must override this method to support dtos.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Compitible DTO.</returns>
        protected virtual IModel? ToDTO(Type type)
        {
            var t = GetType();
            if (type == t || t.IsAssignableTo(type))
                return this;
            return null;
        }
        IModel? IExModelToDTO.ToDTO(Type type) =>
            ToDTO(type);
#else
        /// <summary>
        /// Provides compitible DTO of given type from this model.
        /// You must override this method to support dtos.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Compitible DTO.</returns>
        protected virtual IModel? ToDTO(Type type) => this;

#endif
        //+:cnd:noEmit
        #endregion
    }
#endregion

    #region Model<TID>
    /// <summary>
    /// This class represents a base class for any user-defined model.
    /// Highly customizable by using the following conditional compilation symbols:
    /// MODEL_DELETABLE;
    /// MODEL_APPENDABLE;
    /// MODEL_UPDATABLE;
    /// MODEL_USEMYOWNCONTROLLER
    /// </summary>
    public abstract partial class Model<TID, TModel> : Model<TModel>,
        IExModel<TID>, IExModel, ISelfModel<TID, TModel>
        where TID : struct
        where TModel : Model<TID, TModel>
    {
        #region VARIABLES
        TID id;
        #endregion

        #region CONSTRUCTOR
        protected Model(bool generateNewID)
        {
            if (generateNewID)
                id = GetNewID();
        }
        #endregion

        #region PROPERTIES
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public TID ID { get => id; protected set => id = value; }
        TID IExModel<TID>.ID { get => id; set => id = value; }
        #endregion

        #region PARSE
        Message IExParamParser.Parse(ObjParameter parameter, out object? currentValue, out object? parsedValue, bool updateValueIfParsed, Criteria criteria)
        {
            var name = parameter.Name;
            parsedValue = null;
            object? valueFromParameter;

            switch (name)
            {
                case nameof(ID):
                    currentValue = id;
                    valueFromParameter = parameter is ModelParameter ? ((ModelParameter)parameter).FirstValue : parameter.Value;
                    switch (criteria)
                    {
                        case Criteria.Occurs:
                        case Criteria.BeginsWith:
                        case Criteria.EndsWith:
                        case Criteria.OccursNoCase:
                        case Criteria.BeginsWithNoCase:
                        case Criteria.EndsWithNoCase:
                        case Criteria.StringEqual:
                        case Criteria.StringEqualNoCase:
                        case Criteria.StringNumGreaterThan:
                        case Criteria.StringNumLessThan:
                        case Criteria.NotOccurs:
                        case Criteria.NotBeginsWith:
                        case Criteria.NotEndsWith:
                        case Criteria.NotOccursNoCase:
                        case Criteria.NotBeginsWithNoCase:
                        case Criteria.NotEndsWithNoCase:
                        case Criteria.NotStrEqual:
                        case Criteria.NotStrEqualNoCase:
                        case Criteria.NotStringGreaterThan:
                        case Criteria.NotStringLessThan:
                            parsedValue = valueFromParameter?.ToString();
                            return Message.Sucess(name);
                        default:
                            break;
                    }
                    if (valueFromParameter == null)
                        goto EXIT_ID;
                    if (((IExModel<TID>)this).TryParseID(valueFromParameter, out TID newID))
                    {
                        parsedValue = newID;
                        if (updateValueIfParsed && Equals(id, default(TID)))
                            id = newID;

                        return Message.Sucess(name);
                    }
                    EXIT_ID:
                    return Message.Ignored(name);
                default:
                    switch (criteria)
                    {
                        case Criteria.Occurs:
                        case Criteria.BeginsWith:
                        case Criteria.EndsWith:
                        case Criteria.OccursNoCase:
                        case Criteria.BeginsWithNoCase:
                        case Criteria.EndsWithNoCase:
                        case Criteria.StringEqual:
                        case Criteria.StringEqualNoCase:
                        case Criteria.StringNumGreaterThan:
                        case Criteria.StringNumLessThan:
                        case Criteria.NotOccurs:
                        case Criteria.NotBeginsWith:
                        case Criteria.NotEndsWith:
                        case Criteria.NotOccursNoCase:
                        case Criteria.NotBeginsWithNoCase:
                        case Criteria.NotEndsWithNoCase:
                        case Criteria.NotStrEqual:
                        case Criteria.NotStrEqualNoCase:
                        case Criteria.NotStringGreaterThan:
                        case Criteria.NotStringLessThan:
                            valueFromParameter = parameter is ModelParameter ? ((ModelParameter)parameter).FirstValue : parameter.Value;
                            parsedValue = valueFromParameter?.ToString();
                            Parse(parameter, out currentValue, out _, updateValueIfParsed);
                            return Message.Sucess(name);
                        default:
                            break;
                    }
                    break;
            }
            return Parse(parameter, out currentValue, out parsedValue, updateValueIfParsed);
        }
        #endregion

        #region COPY FROM
        Task<bool> IExCopyable.CopyFrom(IModel model)
        {
            if (model is IModel<TID>)
            {
                var m = (IModel<TID>)model;
                if (Equals(id, default(TID)))
                    id = m.ID;
                return CopyFrom(model);
            }

            //-:cnd:noEmit
#if MODEL_USEDTO
            if (model is IModel)
            {
                if (Equals(id, default(TID)))
                    id = GetNewID();
                return CopyFrom(model);
            }
#endif
            //+:cnd:noEmit
            return Task.FromResult(false);
        }
        #endregion

        #region GET NEW ID
        protected abstract TID GetNewID();
        TID IExModel<TID>.GetNewID() =>
            GetNewID();
        #endregion

        #region TRY PARSE ID
        /// <summary>
        /// Tries to parse the given value to the type of ID
        /// Returns parsed value if succesful, otherwise default value.
        /// </summary>
        /// <param name="value">Value to be parsed as TIDType.</param>
        /// <param name="newID">Parsed value.</param>
        /// <returns>True if succesful, otherwise false</returns>
        protected abstract bool TryParseID(object value, out TID newID);
        bool IExModel<TID>.TryParseID(object value, out TID newID)
        {
            if (value is TID)
            {
                newID = (TID)value;
                return true;
            }
            if (value == null)
            {
                newID = default(TID);
                return false;
            }            
            return TryParseID(value, out newID);
        }
        #endregion
    }
    #endregion
}