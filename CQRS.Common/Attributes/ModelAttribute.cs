/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/
using CQRS.Common.Models;

namespace CQRS.Common.Attributes
{
    #region MODEL ATTRIBUTE
    /// <summary>
    /// Provides various options for a model to represent itself through Web API. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Interface, AllowMultiple = false)]
    public class ModelAttribute : Attribute
    {
        #region CONSTRUCTORS
        public ModelAttribute()
        {
            AutoController = true;
        }
        public ModelAttribute(string? name, bool autoController = true)
        {
            Name = name;
            AutoController = autoController;
        }
        public ModelAttribute(string? name, ServiceScope scope, bool autoController = true):
            this(name, autoController)
        {
            Scope = scope;
        }
        #endregion

        #region PROPERTIES
        /// <summary>
        /// Gets or sets Name of a dynamic controller to be used for a model which uses this attribute.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets a scope of service to be used for a model which uses this attribute.
        /// </summary>
        public ServiceScope Scope { get; set; }

        /// <summary>
        /// Gets or sets a flag to choose whether to use a dynamic controller or not for a model which uses this attribute.
        /// </summary>
        public bool AutoController { get; set; }
        #endregion
    }
    #endregion
}
