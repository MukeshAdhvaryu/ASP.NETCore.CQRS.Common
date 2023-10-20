using System.Reflection;

using CQRS.Common.Attributes;
using CQRS.Common.Interfaces;
using CQRS.Common.Models;

namespace CQRS.Common.Services
{
    #region IContract
    /// <summary>
    /// This interface represents a contract of operations.
    /// </summary>
    public interface IContract : IModelCount
    {
        /// <summary>
        /// Gets a name of an associated model.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        ContractKind Kind { get; }
    }
    #endregion

    #region IExContract
    /// <summary>
    /// This interface represents an internal contract of operations.
    /// </summary>
    partial interface IExContract : IContract
    { }
    #endregion

    #region CONTRACT
    public abstract partial class Contract: IExContract  
    {
        #region VARIABLES
        protected readonly string Name;
        #endregion

        #region CONSTRUCTOR
        protected Contract(Type modelType)
        {
            Name = GetType().GetName(modelType, Kind);
        }
        #endregion

        #region PROPERTIES
        string IContract.Name => Name;
        public abstract ContractKind Kind { get; }
        #endregion

        #region GET URL
        protected string GetUrl(string suffix)=>
            string.Format(Globals.Url, Name, suffix);
        #endregion

        #region GET MODEL COUNT
        public abstract int GetModelCount();
        #endregion
    }
    #endregion
}
