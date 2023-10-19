/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using CQRS.Common.Interfaces;

namespace CQRS.Common.Models
{
    #region IModel
    /// <summary>
    /// This interface represents a model.
    /// Highly customizable by using the following conditional compilation symbols:
    /// MODEL_DELETABLE;
    /// MODEL_APPENDABLE;
    /// MODEL_UPDATABLE;
    /// MODEL_USEMYOWNCONTROLLER
    /// </summary>
    public interface IModel
    { }
    #endregion

    #region IExModel
    /// <summary>
    /// This interface represents a model.
    /// </summary>
    internal partial interface IExModel : IModel, IExCopyable, IExParamParser, IExModelExceptionSupplier
    //-:cnd:noEmit
#if MODEL_USEDTO
        , IExModelToDTO
#endif
#if MODEL_SEARCHABLE
        , IMatch
#endif
    //+:cnd:noEmit
    {
        /// <summary>
        /// Provides a list of names of properties - must be handled while copying from data supplied from model binder's BindModelAsync method.
        /// If the list is not provided, System.Reflecteion will be used to obtain names of the properties defined in this model.
        /// </summary>
        IReadOnlyList<string> GetPropertyNames(bool forSearch = false);

        /// <summary>
        /// Gets initial data.
        /// </summary>
        /// <returns>IEnumerable\<IModel\> containing list of initial data.</returns>
        IEnumerable<IModel> GetInitialData();
    }
    #endregion

    #region IModel<TID>
    /// <summary>
    /// This interface represents a model.
    /// Highly customizable by using the following conditional compilation symbols:
    /// MODEL_DELETABLE;
    /// MODEL_APPENDABLE;
    /// MODEL_UPDATABLE;
    /// MODEL_USEMYOWNCONTROLLER
    /// </summary>
    /// <typeparam name="TID"></typeparam>
    public interface IModel<TID> : IModel
        //-:cnd:noEmit
#if MODEL_SEARCHABLE
        , IMatch
#endif
        //+:cnd:noEmit
        where TID : struct
    {
        /// <summary>
        /// Gets primary key value of this model.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        TID ID { get; }
    }
#endregion

    #region IExModel<TID>
    /// <summary>
    /// This interface represents a model with primary key named as ID.
    /// Highly customizable by using the following conditional compilation symbols:
    /// MODEL_DELETABLE;
    /// MODEL_APPENDABLE;
    /// MODEL_UPDATABLE;
    /// MODEL_USEMYOWNCONTROLLER
    /// </summary>
    /// <typeparam name="TID"></typeparam>
    internal interface IExModel<TID> : IModel<TID>, IExModel
        where TID : struct
    {
        /// <summary>
        /// gets primary key value of this model.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        new TID ID { get; set; }

        /// <summary>
        /// Gets unique id.
        /// </summary>
        /// <returns>Newly generated id.</returns>
        TID GetNewID();

        /// <summary>
        /// Tries to parse the given value to the type of ID
        /// Returns parsed value if succesful, otherwise default value.
        /// </summary>
        /// <param name="value">Value to be parsed as TID.</param>
        /// <param name="newID">Parsed value.</param>
        /// <returns>True if succesful, otherwise false</returns>
        bool TryParseID(object value, out TID newID);
    }
    #endregion

    #region ISelfModel<TModel>
    /// <summary>
    /// This interface represents a self-referencing model.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public partial interface ISelfModel<TModel> : IModel
        //-:cnd:noEmit
#if MODEL_SEARCHABLE
        , IMatch
#endif
        //+:cnd:noEmit
        where TModel : ISelfModel<TModel>
    {
    }
    #endregion

    #region ISelfModel<TModel>
    /// <summary>
    /// This interface represents a self-referencing model with the primary key of type TID.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public partial interface ISelfModel<TID, TModel> : ISelfModel<TModel>, IModel<TID>
        where TModel : ISelfModel<TID, TModel>, IModel<TID>
        where TID : struct
    {
    }
    #endregion
}