/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
Author: Mukesh Adhvaryu.
*/

namespace CQRS.Common.Models
{
    #region ModelInt32
    /// <summary>
    /// Represents a model with interger type primary key.
    /// </summary>
    /// <typeparam name="TModel">Type of user-defined model</typeparam>
    public abstract class ModelInt32<TModel> : Model<int, TModel>
        where TModel : ModelInt32<TModel>
    {
        #region VARIABLES
        static volatile int IDCounter;
        #endregion

        #region CONSTRUCTORS
        protected ModelInt32() :
            base(false)
        { }
        protected ModelInt32(bool generateNewID) :
            base(generateNewID)
        { }
        #endregion

        #region GET NEW ID
        protected override int GetNewID()
        {
            return ++IDCounter;
        }
        #endregion

        #region TRY PARSE ID
        protected override bool TryParseID(object value, out int newID)
        {
            return int.TryParse(value.ToString(), out newID);
        }
        #endregion
    }
    #endregion
}
