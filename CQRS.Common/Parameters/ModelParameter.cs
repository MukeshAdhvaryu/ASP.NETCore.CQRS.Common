/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/
using System.Collections;

namespace CQRS.Common.Parameters
{
    #region MODEL PARAMETER
    /// <summary>
    /// Represents a store which contains values of type T.
    /// </summary>
    /// <typeparam name="T">Any type.</typeparam>
    public class ModelParameter : ObjParameter, IReadOnlyList<string>
    {
        readonly IReadOnlyList<string> Items;

        public ModelParameter() { }
        public ModelParameter(IReadOnlyList<string> values, string _name) :
            base(values, _name)
        { 
            Items = values;
        }
        public ModelParameter(string _name) :
            this(new string[0], _name)
        { 
        }

        public int Count => Items.Count;

        public string this[int index] => Items[index];

        /// <summary>
        /// Gets the first value if this object contains collection of values.
        /// </summary>
       public string FirstValue => Items.Count> 0? Items[0]: "";

        public IEnumerator<string> GetEnumerator() =>
            Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
    #endregion
}
