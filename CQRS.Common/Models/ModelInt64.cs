/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
Author: Mukesh Adhvaryu.
*/

namespace CQRS.Common.Models
{
    /// <summary>
    /// Represents a model with long type primary key.
    /// </summary>
    /// <typeparam name="TModel">Type of user-defined model</typeparam>
    public abstract class ModelInt64<TModel> : Model<long, TModel>
        where TModel : ModelInt64<TModel>
    {
        #region VARIABLES
        static long IDCounter;
        #endregion

        #region CONSTRUCTORS
        protected ModelInt64() :
            base(false)
        { }
        protected ModelInt64(bool generateNewID) :
            base(generateNewID)
        { }
        #endregion

        #region GET NEW ID
        protected override long GetNewID()
        {
            return ++IDCounter;
        }
        #endregion
    }
}
