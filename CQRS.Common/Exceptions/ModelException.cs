/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/

using CQRS.Common.Models;

namespace CQRS.Common.Exceptions
{
    #region IMODEL EXCEPTION
    /// <summary>
    /// Represents an exception object customized for model operations.
    /// </summary>
    public interface IModelException
    {
        /// <summary>
        /// Gets type of this exception.
        /// </summary>
        ExceptionType Type { get; }

        /// <summary>
        /// Gets message of this exception.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Gets status of operation which this exception was originated from.
        /// </summary>
        int Status { get; }

        /// <summary>
        /// Gets inner exception (additional details).
        /// </summary>
        Exception? InnerException { get; }

        /// <summary>
        /// Gets consolidated message elements from this exception.
        /// </summary>
        /// <param name="title">Returns title of the message.</param>
        /// <param name="type">Returns type of the message.</param>
        /// <param name="details">Returns details of the message.</param>
        /// <param name="isProductionEnvironment">If true, details returned with the message will be brief, otherwise thorough.</param>
        void GetConsolidatedMessage(out string title, out string type, out string? details, out string[]? stackTrace,  bool isProductionEnvironment = false);
    }
    #endregion

    #region MODEL EXCEPTION
    /// <summary>
    /// Represents an exception object customized for model operations.
    /// </summary>
    public class ModelException: Exception, IModelException
    {
        #region CONSTRUCTORS
        public ModelException(string message, ExceptionType type) :
            base(message)
        {
            Type = type;
        }
        public ModelException(ExceptionType type, Exception exception) :
            base(exception.Message, exception)
        {
            Type = type;
        }
        public ModelException(string message, ExceptionType type, Exception exception) :
           base(message, exception)
        {
            Type = type;
        }
        #endregion

        #region PROPERTIES
        public ExceptionType Type { get; }

        public virtual int Status
        {
            get
            {
                switch (Type)
                {
                    case ExceptionType.Unknown:
                    case ExceptionType.NoModelFound:
                    case ExceptionType.NoModelFoundForID:
                    case ExceptionType.NoModelsFound:
                        return 404;
                    case Models.ExceptionType.NoModelSupplied:
                    case ExceptionType.NegativeFetchCount:
                    case ExceptionType.ModelCopyOperationFailed:
                    case ExceptionType.NoParameterSupplied:
                    case ExceptionType.NoParametersSupplied:
                    case ExceptionType.AddOperationFailed:
                    case ExceptionType.UpdateOperationFailed:
                    case ExceptionType.DeleteOperationFailed:
                        return 400;
                    case ExceptionType.InternalServerError:
                        return 500;
                    default:
                        return 400;
                }
            }
        }
        #endregion

        #region GET CONSOLIDATED MESSAGE
        public void GetConsolidatedMessage(out string Title, out string Type, out string? Details, out string[]? stackTrace, bool isProductionEnvironment = false)
        {
            Title = Message;
            Type = this.Type.ToString();
            Details =  "None" ;
            stackTrace = new string[] { };

            if (InnerException != null)
            {
                Details = InnerException.Message;

                if (InnerException.StackTrace != null && !isProductionEnvironment)
                {
                    stackTrace = InnerException.StackTrace.Split(System.Environment.NewLine);
                }
            }
        }
        #endregion

        #region CREATE
        /// <summary>
        /// Creates an instance of ModelException.
        /// </summary>
        /// <param name="message">Custom message provided by user.</param>
        /// <param name="type">Type of the model exception.</param>
        /// <param name="exception">Original exception raised by some operation performed on model.</param>
        /// <returns></returns>
        public static ModelException Create(string message, ExceptionType type, Exception? exception = null)
        {
            if(exception == null) 
                return new ModelException(message, type);
            return new ModelException(message, type, exception);
        }
        #endregion
    }
    #endregion
}
