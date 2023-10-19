/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
Author: Mukesh Adhvaryu.
*/

namespace CQRS.Common.Models
{
    #region ModelEnum
    /// <summary>
    /// Represents a model with enum type primary key.
    /// </summary>
    /// <typeparam name="TModel">Type of user-defined model</typeparam>
    public abstract class ModelEnum<TEnum, TModel> : Model<TEnum, TModel>
        where TModel : ModelEnum<TEnum, TModel>
        where TEnum : struct, Enum
    {
        #region VARIABLES
        static HashSet<TEnum> Used  = new HashSet<TEnum>();
        static List<TEnum> Available = new List<TEnum>();
        static volatile int index;
        #endregion

        #region CONSTRUCTORS
        static ModelEnum()
        {
            Available = new List<TEnum>(Enum.GetValues<TEnum>());
        }
        protected ModelEnum() :
            base(false)
        { }
        protected ModelEnum(bool generateNewID) :
            base(generateNewID)
        { }
        #endregion

        #region GET NEW ID
        protected override TEnum GetNewID()
        {
           if(Available.Count == 0)
                return default(TEnum);
            var newID = Available[index];
            if (!Used.Contains(newID))
            {
                Used.Add(newID);
                return newID;
            }
            while (index < Available.Count)
            {
                newID = Available[index++];

                if (!Used.Contains(newID))
                {
                    Used.Add(newID);
                    return newID;
                }
            }
            return default(TEnum);
        }
        #endregion

        #region TRY PARSE ID
        protected override bool TryParseID(object value, out TEnum newID)
        {
            return Enum.TryParse(value.ToString(), true, out newID);
        }
        #endregion
    }
    #endregion
}
