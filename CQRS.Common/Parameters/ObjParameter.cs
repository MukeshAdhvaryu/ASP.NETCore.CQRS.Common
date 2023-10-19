/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/

namespace CQRS.Common.Parameters
{
    /// <summary>
    /// Represents an object which serves as a parameter to aid some operation.
    /// </summary>
    public class ObjParameter
    {
        public ObjParameter() { }
        public ObjParameter(object? value, string name)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Gets or sets name of this object.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets or sets value of this object.
        /// </summary>
        public object? Value { get; }
    }
}
