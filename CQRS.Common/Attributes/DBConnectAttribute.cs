/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/
using CQRS.Common.Models;

namespace CQRS.Common.Attributes
{
    #region DBCONNECT ATTRIBUTE
    /// <summary>
    /// Provides various DB connect options for a model. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DBConnectAttribute : Attribute
    {
        #region CONSTRUCTORS
        public DBConnectAttribute(bool provideSeedData = false, ConnectionKey connectionKey = 0, string? database = null)
        {
            ProvideSeedData = provideSeedData;
            ConnectionKey = connectionKey;
            Database = database;
        }
        #endregion

        #region PROPERTIES
        /// <summary>
        /// Gets or sets a flag to indicate whether or not to provide an initial collection of models when model collection is empty for a model which uses this attribute.
        /// </summary>
        public bool ProvideSeedData { get; set; }

        /// <summary>
        /// Gets or sets a connection key to use a connection with a particular database via DBContext for a model which uses this attribute.
        /// </summary>
        public ConnectionKey ConnectionKey { get; set; }

        /// <summary>
        /// Gets or sets a name of database which a model wants to connect to.
        /// </summary>
        public string? Database { get; set; }
        #endregion
    }
    #endregion
}
