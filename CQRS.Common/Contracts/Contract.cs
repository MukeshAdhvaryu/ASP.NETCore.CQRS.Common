using System.Reflection;

using CQRS.Common.Attributes;
using CQRS.Common.Interfaces;

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
        static HashSet<string> Names;
        static readonly object NamesLock = new object();

        protected readonly string Name;
        static readonly string Url;
        #endregion

        #region CONSTRUCTOR
        static Contract()
        {
            lock(NamesLock)
            {
                Names = new HashSet<string>(3);
                Url = @"/{0} /{1}";
            }  
        }
        public Contract(Type modelType)
        {
            Name = GetName(GetType(), modelType, Kind);
        }
        #endregion

        #region PROPERTIES
        string IContract.Name => Name;
        public abstract ContractKind Kind { get; }
        #endregion

        #region GET URL
        protected string GetUrl(string suffix)=>
            string.Format(Url, Name, suffix);
        #endregion

        #region GET MODEL COUNT
        public abstract int GetModelCount();
        #endregion

        #region GET NAME
        static string GetCleanName(Type type)
        {
            //Let's check if a specific name for the associated controller is provided as an attribute of TModel.
            //If provided, no do further, assign name and exit.
            var nameAttribute = type.GetCustomAttribute<ModelAttribute>();
            if (!string.IsNullOrEmpty(nameAttribute?.Name))
                return nameAttribute.Name;

            var name = type.Name;

            //If TModel is an interface remove 'I' suffix from the name.
            //if (type.IsInterface && name.Length > 1 && char.IsUpper(name[1]) && (name[0] == 'I' || name[0] == 'i'))
            //    name = name.Substring(1);

            if (name.Length == 1 || !type.IsGenericType)
            {
                return name;
            }
            //If TModel is genereic type, remove part representing generic name for example: name of Any<T> resolves in Any `1 as name of the type.
            var idx = name.IndexOf('`');
            if (idx != -1)
                name = name.Substring(0, idx);
            return name;
        }

        static string GetName(Type contractType, Type modelType, ContractKind kind)
        {
            string finalName;
            var modelAttr = modelType.GetCustomAttribute<ModelAttribute>();
            finalName = GetCleanName(modelType);
            if (!string.IsNullOrEmpty(modelAttr?.Name) && !Names.Contains(modelAttr.Name))
                finalName = modelAttr.Name;
            if (string.IsNullOrEmpty(finalName))
                finalName = GetCleanName(modelType);

            finalName += kind;
            int count = contractType.GenericTypeArguments.Length;
            if (count > 1)
            {
                int i = -1;
                while (Names.Contains(finalName))
                {
                    ++i;
                    if (i == 2)
                        continue;
                    if (i >= count)
                        break;
                    modelType = contractType.GenericTypeArguments[i];
                    modelAttr = modelType.GetCustomAttribute<ModelAttribute>();
                    finalName = GetCleanName(modelType);
                    if (!string.IsNullOrEmpty(modelAttr?.Name) && !Names.Contains(modelAttr.Name))
                        finalName = modelAttr.Name + kind;
                }
            }
            Names.Add(finalName);
            return finalName;
        }
        #endregion
    }
    #endregion
}
