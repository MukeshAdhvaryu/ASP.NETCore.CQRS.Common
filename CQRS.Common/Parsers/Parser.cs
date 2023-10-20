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
        [Flags]
        enum TypeInfo : byte
        {
            None,
            IsEnumerable = 0x1,
            IsParseable = 0x2,
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
            if(Type.IsAssignableTo(typeof(ISelfParser<T>)) && (Type.IsValueType || Type.GetConstructor(Type.EmptyTypes) != null))
                Info |= TypeInfo.IsParseable;
        }
        public Parser(T? result)
        {
            Result = result;
        }
        #endregion

        #region TRY PARSE
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
            if ((Info & TypeInfo.IsParseable) == TypeInfo.IsParseable)
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
