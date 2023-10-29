/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/
using System.Collections;

using CQRS.Common.Interfaces;

namespace CQRS.Common.Parsers
{
    public readonly struct Parser<T>: IParser
    {
        /// <summary>
        /// Indicates of T is enumerable and/or implements ISelfParser<typeparamref name="T"/> by itself.
        /// </summary>
        [Flags]
        enum TypeInfo : byte
        {
            None,

            /// <summary>
            /// Indicates that the type is an IEnumerable
            /// </summary>
            IsEnumerable = 0x1,

            /// <summary>
            /// Indicates that the type is parsable
            /// </summary>
            IsParsable = 0x2,
        }

        #region VARIABLES
        public readonly T? Result;
        static readonly Type Type;
        static readonly TypeInfo Info;
        #endregion

        #region CONSTRUCTORS
        static Parser()
        {
            Type = typeof(T);
            Info = 0;

            if (Type.IsAssignableTo(typeof(IEnumerable)))
                Info |= TypeInfo.IsEnumerable;

            //Check if type is not abstract and has a parameter-less constructor and implements ISelfParser<T> interface.
            if (Type.IsAssignableTo(typeof(ISelfParser<T>)) && (Type.IsValueType || Type.GetConstructor(Type.EmptyTypes) != null))
                Info |= TypeInfo.IsParsable;
        }
        public Parser(T? result)
        {
            Result = result;
        }
        #endregion

        #region TRY PARSE
        /// <summary>
        /// Parses json string into a concrete Parser<typeparamref name="T"/> instance.
        /// This method is required in order to API end-points to work without throwing an error.
        /// </summary>
        /// <param name="json">Json string to parse into an instance of type of T</param>
        /// <param name="parser">Concrete instance of Parser<typeparamref name="T"/> class which contains an instance of T.</param>
        /// <returns></returns>
        public static bool TryParse(string json, out Parser<T> parser)
        {
            json = json.Trim();
            if ((Info & TypeInfo.IsEnumerable) == TypeInfo.IsEnumerable)
            {
                if (!json.StartsWith("["))
                    json = "[" + json;
                if (!json.EndsWith("]"))
                    json += "]";
            }
            T? result;
            if ((Info & TypeInfo.IsParsable) == TypeInfo.IsParsable)
            {
                var parsable = Activator.CreateInstance(Type);
                if(parsable != null)
                {
                    var instance = (ISelfParser<T>)parsable;
                    result = instance.Parse(json);
                    parser = new Parser<T>(result);
                    return true;
                }
            } 
            result = Globals.Parse<T>(json, Type);
            parser = new Parser<T>(result);
            return true;
        }
        #endregion
    }
}
