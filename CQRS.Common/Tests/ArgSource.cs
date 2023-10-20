/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
Author: Mukesh Adhvaryu.
*/

//-:cnd:noEmit
#if MODEL_ADDTEST
//+:cnd:noEmit
using System.Collections;

namespace CQRS.Common.Tests
{
    #region ArgSource
    /// <summary>
    /// Represents an object which provides a collection of argumets to use in a test method.
    /// A user must override property Data and provide an apprpriate set of arguments.
    /// </summary>
    public abstract class ArgSource: IEnumerable<object[]>
    {
        /// <summary>
        /// Gets a collection of object arrays to provide for arguments.
        /// </summary>
        public abstract IEnumerable<object[]> Data {get; }
        public IEnumerator<object[]> GetEnumerator() =>
            Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            Data.GetEnumerator();
    }
    #endregion
}
//-:cnd:noEmit
#endif
//+:cnd:noEmit