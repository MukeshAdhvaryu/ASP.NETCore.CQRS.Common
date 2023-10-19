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
    public class SearchParameter: ObjParameter
    {
        public static readonly SearchParameter Empty = new SearchParameter("", null);

        public SearchParameter(){}
        public SearchParameter(string name, object? value, Criteria criteria = Criteria.Equal):
            base(value, name)
        {
            Criteria = criteria;
        }

        /// <summary>
        /// Gets criteria for the intended search.
        /// </summary>
        public Criteria Criteria { get; }
    }
}
#endif
//-:cnd:noEmit

