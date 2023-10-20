/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

using CQRS.Common.Interfaces;

namespace CQRS.Common.Models
{
    #region MODEL
    public abstract class Model : IExParamParser, IEntity
    {
        #region PROPERTIES
        public virtual object? this[string? propertyName]
        {
            get
            {
                if (string.IsNullOrEmpty(propertyName))
                    return false;
                var property = GetType().GetProperty(propertyName, BindingFlags.IgnoreCase);
                if (property == null)
                    return false;

                return property.GetValue(this, null);
            }
        }
        #endregion

        #region PARSE
        /// <summary>
        /// Parses the specified parameter and if possible emits the value compitible with
        /// the property this object posseses.
        /// </summary>
        /// <param name="propertyName">Name of the property which to parse the value against.</param>
        /// <param name="propertyValue">Value to be parsed to obtain compitible value.</param>
        /// <param name="parsedValue">If succesful, a compitible value parsed using supplied value from parameter.</param>
        /// <param name="updateValueIfParsed">If succesful, replace the current value with the compitible parsed value.</param>
        /// <returns>Result Message with Status of the parse operation.</returns>
        /// <returns></returns>
        protected abstract bool Parse(string? propertyName, object? propertyValue, out object? parsedValue, bool updateValueIfParsed);


        bool IExParamParser.Parse(string? propertyName, object? propertyValue, out object? parsedValue, bool updateValueIfParsed, Criteria criteria)
        {
            parsedValue = null;
            if (string.IsNullOrEmpty(propertyName) || propertyValue == null)
            {
                return false;
            }

            if (propertyValue is JsonElement)
            {
                var jsonValue = (JsonElement)propertyValue;
                switch (jsonValue.ValueKind)
                {
                    case JsonValueKind.Undefined:
                    case JsonValueKind.Null:
                        return false;
                    case JsonValueKind.Object:
                        goto PARSE;
                    case JsonValueKind.Array:
                        parsedValue = ParseCollection(propertyName, Globals.Parse<object[]>(jsonValue), updateValueIfParsed, criteria);
                        return parsedValue != null;
                    case JsonValueKind.String:
                    case JsonValueKind.Number:
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        goto PARSE;
                    default:
                        break;
                }
            }

            propertyName = propertyName.ToLower();

            if (propertyValue is IReadOnlyCollection<string>)
            {
                parsedValue = ParseCollection(propertyName, (IReadOnlyCollection<string>)propertyValue, updateValueIfParsed, criteria);
                return parsedValue != null;
            }
            PARSE:
            return Parse(propertyName, propertyValue, out parsedValue, updateValueIfParsed, criteria);
        }

        bool Parse(string propertyName, object? propertyValue, out object? parsedValue, bool updateValueIfParsed, Criteria criteria)
        {
            parsedValue = null;
            if (string.IsNullOrEmpty(propertyName) || propertyValue == null)
            {
                return false;
            }
            propertyName = propertyName.ToLower();

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
                case Criteria.StringGreaterThan:
                case Criteria.StringLessThan:
                    parsedValue = propertyValue.ToString();
                    return true;
                default:
                    break;
            }
            return Parse(propertyName.ToLower(), propertyValue, out parsedValue, updateValueIfParsed);
        }
        object? ParseCollection<T>(string propertyName, IReadOnlyCollection<T>? valueCollection, bool updateValueIfParsed, Criteria criteria)
        {
            if (valueCollection == null || valueCollection.Count == 0)
            {
                return null;
            }
            if (valueCollection.Count == 1)
            {
                object? obj = valueCollection.FirstOrDefault();
                if (obj == null)
                    return null;
                Parse(propertyName, obj, out obj, updateValueIfParsed, criteria);
                return obj;
            }
            List<object?> results = new List<object?>();

            foreach (var item in valueCollection)
            {
                if (Parse(propertyName, item, out object? rslt, updateValueIfParsed, criteria) && rslt != null)
                {
                    results.Add(rslt);
                }
            }
            if (results.Count > 0)
                return results.ToArray();
            return null;
        }
        #endregion
    }
    #endregion

    #region Model<TModel>
    public abstract partial class Model<TModel> : Model, ISelfModel<TModel>, IExModel
        where TModel : Model<TModel>, ISelfModel<TModel>
    {
        #region VARIABLES
        public readonly string ModelName;
        #endregion

        #region CONSTRUCTOR
        protected Model()
        {
            var type = GetType();
            ModelName = type.Name;
        }
        #endregion

        #region PROPERTIES
        public virtual object? this[string? propertyName]
        {
            get 
            {
                if (string.IsNullOrEmpty(propertyName))
                    return false;
                var property = GetType().GetProperty(propertyName, BindingFlags.IgnoreCase);
                if (property == null)
                    return false;

                return property.GetValue(this, null);
            }
        }
        #endregion

        #region COPY FROM
        /// <summary>
        /// Copies data from the given model to this instance.
        /// </summary>
        /// <param name="model">Model to copy data from.</param>
        /// <returns></returns>
        protected abstract Task<Tuple<bool, string>> CopyFrom(IModel model);
        Task<Tuple<bool, string>> IExCopyable.CopyFrom(IModel model) =>
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

        #region GET APPROPRIATE EXCEPTION MESSAGE
        /// <summary>
        /// Supplies an appropriate exception message for a failure in a specified method.
        /// </summary>
        /// <param name="exceptionType">Type of exception to get.</param>
        /// <param name="additionalInfo">Additional information to aid the task of exception supply.</param>
        /// <returns>Exception message.</returns>
        protected virtual string GetModelExceptionMessage(ExceptionType exceptionType, string? additionalInfo = null)
        {
            bool noAdditionalInfo = string.IsNullOrEmpty(additionalInfo);

            switch (exceptionType)
            {
                case ExceptionType.NoModelFound:
                case ExceptionType.NoModelFoundForID:
                    return (string.Format("No {0} is found additional info: {1}", ModelName, noAdditionalInfo ? "None" : "ID = " + additionalInfo));

                case ExceptionType.NoModelsFound:
                    return (string.Format("No {0} are found additional info: {1}", ModelName, additionalInfo ?? " None"));

                case ExceptionType.NoModelSupplied:
                    return (string.Format("No {0} models supplied additional info: {1}", ModelName, additionalInfo ?? " None"));

                case ExceptionType.NoIDsSupplied:
                    return (string.Format("No {0} IDs supplied additional info: {1}", ModelName, additionalInfo ?? " None"));

                case ExceptionType.NoModelsSupplied:
                    return (string.Format("Null {0} can not be supplied additional info: {1}", ModelName, additionalInfo ?? " None"));

                case ExceptionType.NegativeFetchCount:
                    return (string.Format("{0} fetch count must be > 0; provided: {1}", ModelName, additionalInfo ?? " None"));

                case ExceptionType.ModelCopyOperationFailed:
                    return (string.Format("Copy operation of {0} is failed; model provided: {1}", ModelName, additionalInfo ?? " None"));

                case ExceptionType.NoParameterSupplied:
                    return (string.Format("Null parameter for searching a {0} is not allowed; additional info: {1}", ModelName, additionalInfo ?? " None"));

                case ExceptionType.NoParametersSupplied:
                    return (string.Format("Null parameters for searching  {0}s are not allowed; additional info: {1}", ModelName, additionalInfo ?? " None"));

                case ExceptionType.AddOperationFailed:
                    return (string.Format("Add operation for adding new {0} is failed; additional info: {1}", ModelName, additionalInfo ?? " None"));

                case ExceptionType.UpdateOperationFailed:
                    return (string.Format("Update operation for updating the {0} is failed; additional info: {1}", ModelName, additionalInfo ?? " None"));

                case ExceptionType.DeleteOperationFailed:
                    return (string.Format("Delete operation for deleting the {0} is failed; additional info: {1}", ModelName, additionalInfo ?? " None"));

                case ExceptionType.InternalServerError:
                    return (string.Format("Model {0}: internal server error; additional info: {1}", ModelName, additionalInfo ?? " None"));

                case ExceptionType.ExpectationFailed:
                    return (string.Format("Model {0}: expectation failed; additional info: {1}", ModelName, additionalInfo ?? " None"));

                case ExceptionType.InvalidContext:
                    return (string.Format("The supplied model context is not valid or compitible with the {0}", additionalInfo ?? " None"));

                case ExceptionType.InAppropriateModelSupplied:
                    return (string.Format("Inappropriate {0} can not be supplied additional info: {1}", ModelName, additionalInfo ?? " None"));

                case ExceptionType.MissingValue:
                    return (string.Format("ModelName {0}: Value is not supplied for: {1}", ModelName, additionalInfo ?? " None")); 

                case ExceptionType.MissingRequiredValue:
                    return (string.Format("ModelName {0}: Required Value is not supplied for: {1}", ModelName, additionalInfo ?? " None")); 

                case ExceptionType.IgnoredValue:
                    return (string.Format("ModelName {0}: Value is ignored for: {1}", ModelName, additionalInfo ?? " None")); 

                default:
                    return ("Need to supply your message");
            }
        }

        string IExModelExceptionSupplier.GetModelExceptionMessage(ExceptionType exceptionType, string? additionalInfo) =>
            GetModelExceptionMessage(exceptionType, additionalInfo);
        #endregion

        //-:cnd:noEmit
#if MODEL_USEDTO
        #region IModelToDTO
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
        #endregion
#endif
        //+:cnd:noEmit
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
        static IDType IDType;
        #endregion

        #region CONSTRUCTOR
        static Model()
        {
            var t = typeof(TID);
            if (t == typeof(int))
            {
                IDType = IDType.Int32;
                return;
            }
            if (t == typeof(uint))
            {
                IDType = IDType.UInt32;
                return;
            }
            if (t == typeof(short))
            {
                IDType = IDType.Int16;
                return;
            }
            if (t == typeof(ushort))
            {
                IDType = IDType.UInt16;
                return;
            }

            if (t == typeof(long))
            {
                IDType = IDType.Int64;
                return;
            }
            if (t == typeof(ulong))
            {
                IDType = IDType.UInt64;
                return;
            }
            if (t == typeof(byte))
            {
                IDType = IDType.Byte;
                return;
            }
            if (t == typeof(sbyte))
            {
                IDType = IDType.SByte;
                return;
            }
            if (t == typeof(Enum))
            {
                IDType = IDType.Enum;
                return;
            }
            if (t == typeof(Guid))
            {
                IDType = IDType.Guid;
                return;
            }
        }
        protected Model(bool generateNewID)
        {
            if (generateNewID)
                id = GetNewID();
        }
        #endregion

        #region PROPERTIES
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public TID ID { get => id; set => id = value; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public override object? this[string? propertyName]
        {
            get
            {
                if (propertyName == null)
                    return null;
                switch (propertyName.ToLower())
                {
                    case "id":
                        return ID;
                    default:
                        break;
                }
                return base[propertyName];
            }
        }

        TID IExModel<TID>.ID { get => id; set => id = value; }
        #endregion

        #region COPY FROM
        Task<Tuple<bool, string>> IExCopyable.CopyFrom(IModel model)
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
            return Task.FromResult(Tuple.Create(false, GetModelExceptionMessage(ExceptionType.InAppropriateModelSupplied, model?.ToString())));
        }
        #endregion

        #region TRY PARSE ID
        /// <summary>
        /// Tries to parse the given value to the type of ID
        /// Returns parsed value if succesful, otherwise default value.
        /// </summary>
        /// <param name="propertyValue">Value to be parsed as TIDType.</param>
        /// <param name="id">Parsed value.</param>
        /// <returns>True if succesful, otherwise false</returns>
        protected virtual bool TryParseID(object? propertyValue, out TID id)
        {
            id = default(TID);
            return false;
        }
        bool IExModel<TID>.TryParseID(object? propertyValue, out TID id)
        {
            if (propertyValue is TID)
            {
                id = (TID)(object)propertyValue;
                return true;
            }
            if (propertyValue == null)
            {
                id = default(TID);
                return false;
            }
            var value = propertyValue?.ToString();
            id = default(TID);

            if (string.IsNullOrEmpty(value))
                return false;

            switch (IDType)
            {
                case IDType.Int16:
                    if (short.TryParse(value, out short sResult))
                    {
                        id = (TID)(object)sResult;
                        return true;
                    }
                    break;
                case IDType.Int32:
                    if (int.TryParse(value, out int iResult))
                    {
                        id = (TID)(object)iResult;
                        return true;
                    }
                    break;
                case IDType.Int64:
                    if (long.TryParse(value, out long lResult))
                    {
                        id = (TID)(object)lResult;
                        return true;
                    }
                    break;
                case IDType.Byte:
                    if (byte.TryParse(value, out byte bResult))
                    {
                        id = (TID)(object)bResult;
                        return true;
                    }
                    break;
                case IDType.Enum:
                    if (Enum.TryParse(value, out TID eResult))
                    {
                        id = (TID)(object)eResult;
                        return true;
                    }
                    break;
                case IDType.Guid:
                    if (Guid.TryParse(value, out Guid gResult))
                    {
                        id = (TID)(object)gResult;
                        return true;
                    }
                    break;
                case IDType.UInt16:
                    if (ushort.TryParse(value, out ushort usResult))
                    {
                        id = (TID)(object)usResult;
                        return true;
                    }
                    break;
                case IDType.UInt32:
                    if (uint.TryParse(value, out uint uiResult))
                    {
                        id = (TID)(object)uiResult;
                        return true;
                    }
                    break;
                case IDType.UInt64:
                    if (ulong.TryParse(value, out ulong ulResult))
                    {
                        id = (TID)(object)ulResult;
                        return true;
                    }
                    break;
                case IDType.SByte:
                    if (sbyte.TryParse(value, out sbyte sbResult))
                    {
                        id = (TID)(object)sbResult;
                        return true;
                    }
                    break;
                default:
                    break;
            }
            return TryParseID(propertyValue, out id);
        }
        #endregion

        #region PARSE
        protected override bool Parse(string? propertyName, object? propertyValue, out object? parsedValue, bool updateValueIfParsed)
        {
            propertyName = propertyName?.ToLower();
            switch (propertyName)
            {
                case "id":
                    if (((IExModel<TID>)this).TryParseID(propertyValue, out TID id))
                    {
                        parsedValue = id;
                        if (updateValueIfParsed)
                            this.id = id;
                        return true;
                    }
                    break;
                default:
                    break;
            }
            parsedValue = null;
            return false;
        }
        #endregion

        #region GET NEW ID
        protected abstract TID GetNewID();
        TID IExModel<TID>.GetNewID() =>
            GetNewID();
        #endregion
    }
    #endregion
}