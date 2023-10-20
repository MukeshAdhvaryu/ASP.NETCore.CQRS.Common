/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/
//-:cnd:noEmit
#if MODEL_SEARCHABLE
using CQRS.Common.Models;

namespace CQRS.Common.Parameters
{
    /// <summary>
    /// Reprents a parameter which provides criteria for search.
    /// </summary>
    public interface ISearchParameter 
    {
        /// <summary>
        /// Gets name of this object.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets value of this object.
        /// </summary>
        object? Value { get; }

        /// <summary>
        /// Gets criteria for the intended search.
        /// </summary>
        Criteria Criteria { get; }
    }

    /// <summary>
    /// Reprents a parameter which provides criteria for search.
    /// </summary>
    public struct SearchParameter: ISearchParameter
    {
        public static readonly SearchParameter Empty = new SearchParameter();

        public SearchParameter(string name, Criteria criteria, object? value)
        {
            Name = name;
            Criteria = criteria;
            Value = value;
        }
        public string Name { get; set; }
        public object? Value { get; set; }
        public Criteria Criteria { get; set; }
    }
}
#endif
//-:cnd:noEmit

