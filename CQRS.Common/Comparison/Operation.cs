/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/

//-:cnd:noEmit
#if MODEL_SEARCHABLE

using CQRS.Common.Models;

namespace CQRS.Common
{
    static class Operations 
    {
        #region COMPARE
        /// <summary>
        /// Compares the specified criteria.
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <param name="left">The left value.</param>
        /// <param name="criteria">The criteria.</param>
        /// <param name="right">The right value.</param>
        /// <returns>true if comparison with criteria is successful, otherwise false.</returns>
        public static bool Compare<T>(T left, Criteria criteria, object right) =>
            Operator<T>.Compare(left, right, criteria);

        /// <summary>
        /// Compares the specified criteria.
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <param name="left">The left value.</param>
        /// <param name="criteria">The criteria.</param>
        /// <param name="right">The right value.</param>
        /// <returns>true if comparison with criteria is successful, otherwise false.</returns>
        public static bool Compare<T>(T left, Criteria criteria, T right) =>
            Operator<T>.Compare(left, right, criteria);

        /// <summary>
        /// Compares the specified criteria.
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <param name="criteria">The criteria.</param>
        /// <param name="value1">The value1.</param>
        /// <param name="value2">The value2.</param>
        /// <returns>true if comparison with criteria is successful, otherwise false.</returns>
        public static bool CompareRange<T>(T left, MultCriteria criteria, T value1, T value2) =>
            Operator<T>.CompareRange(left, criteria, value1, value2);

        /// <summary>
        /// Compares the range.
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <param name="left">The left.</param>
        /// <param name="criteria">The criteria.</param>
        /// <param name="value1">The value1.</param>
        /// <param name="value2">The value2.</param>
        /// <returns>true if comparison with criteria is successful, otherwise false.</returns>
        public static bool CompareRange<T>( T left, MultCriteria criteria, object value1, object value2) =>
            Operator<T>.CompareRange(left, criteria, value1, value2);

        /// <summary>
        /// Compares the specified right.
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>true if comparison with criteria is successful, otherwise false.</returns>
        public static bool Compare<T>( T left, T right) =>
            Operator<T>.Compare(left, right, Criteria.Equal);
        #endregion

        #region OPERATOR<T>
        /// <summary>
        /// Class Operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        static class Operator<T>
        {
            #region VARIABLES
            enum Status
            {
                None,
                NotSupported,
                Supported
            }

            /// <summary>
            /// The operation
            /// </summary>
            static Func<T, T, T>[] operation = new Func<T, T, T>[6];

            /// <summary>
            /// The comparison
            /// </summary>
            static Func<T, T, bool>[] comparison = new Func<T, T, bool>[13];

            /// <summary>
            /// The otasks
            /// </summary>
            static Status[] otasks = new Status[6];

            /// <summary>
            /// The ctasks
            /// </summary>
            static Status[] ctasks = new Status[13];
            #endregion

            #region PRIVATE METHODS
            /// <summary>
            /// Creates the operator.
            /// </summary>
            /// <param name="OperatorType">Type of the operator.</param>
            /// <param name="id">The identifier.</param>
            static void CreateOperator(Criteria OperatorType, int id)
            {
                Func<T, T, bool> func = null;

                try
                {
                    switch (OperatorType)
                    {
                        case Criteria.Equal:
                        case Criteria.NotEqual:
                            func = new Func<T, T, bool>(Operator<T>.Equal);
                            break;
                        case Criteria.GreaterThan:
                        case Criteria.NotGreaterThan:
                            func = new Func<T, T, bool>(Operator<T>.Greater);
                            break;
                        case Criteria.LessThan:
                        case Criteria.NotLessThan:
                            func = new Func<T, T, bool>(Operator<T>.Less);
                            break;
                        case Criteria.Occurs:
                        case Criteria.NotOccurs:
                            func = new Func<T, T, bool>(Operator<T>.Occurs);
                            break;
                        case Criteria.BeginsWith:
                        case Criteria.NotBeginsWith:
                            func = new Func<T, T, bool>(Operator<T>.Begins);
                            break;
                        case Criteria.EndsWith:
                        case Criteria.NotEndsWith:
                            func = new Func<T, T, bool>(Operator<T>.Ends);
                            break;
                        case Criteria.OccursNoCase:
                        case Criteria.NotOccursNoCase:
                            func = new Func<T, T, bool>(Operator<T>.OccursIgnoreCase);
                            break;
                        case Criteria.BeginsWithNoCase:
                        case Criteria.NotBeginsWithNoCase:
                            func = new Func<T, T, bool>(Operator<T>.BeginsIgnoreCase);
                            break;
                        case Criteria.EndsWithNoCase:
                        case Criteria.NotEndsWithNoCase:
                            func = new Func<T, T, bool>(Operator<T>.EndsIgnoreCase);
                            break;
                        case Criteria.StringEqual:
                        case Criteria.NotStrEqual:
                            func = new Func<T, T, bool>(Operator<T>.StrEqual);
                            break;
                        case Criteria.StringEqualNoCase:
                        case Criteria.NotStrEqualNoCase:
                            func = new Func<T, T, bool>(Operator<T>.StrEqualIgnoreCase);
                            break;
                        case Criteria.StringNumGreaterThan:
                            func = new Func<T, T, bool>(Operator<T>.StrGreater);
                            break;
                        case Criteria.StringNumLessThan:
                            func = new Func<T, T, bool>(Operator<T>.StrLess);
                            break;
                        default:
                            if (id <= 12) { ctasks[id] = Status.NotSupported; comparison[id] = null; }
                            break;
                    }

                    if (id <= 12) { ctasks[id] = Status.Supported; comparison[id] = func; }
                }
                catch
                {
                    //switch (OperatorType)
                    //{
                    //    case Criteria.Equal:
                    //        func = new Func<T, T, bool>(Operator<T>.Equal);
                    //        ctasks[id] = Status.Supported; comparison[id] = func;
                    //        break;
                    //    case Criteria.GreaterThan:
                    //        func = new Func<T, T, bool>(Operator<T>.Greater);
                    //        ctasks[id] = Status.Supported; comparison[id] = func;
                    //        break;
                    //    case Criteria.LessThan:
                    //        func = new Func<T, T, bool>(Operator<T>.Less);
                    //        ctasks[id] = Status.Supported; comparison[id] = func;
                    //        break;
                    //    default:
                    //        if (id <= 12) { ctasks[id] = Status.NotSupported; comparison[id] = null; }
                    //        break;
                    //}
                }
            }
            /// <summary>
            /// Equals the specified a.
            /// </summary>
            /// <param name="a">a.</param>
            /// <param name="b">The b.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            static bool Equal(T a, T b)
            {
                bool a_ = (object)a == null;
                bool b_ = (object)b == null;

                if (a_ && b_)
                {
                    return true;
                }
                else if (a_)
                {
                    return false;
                }
                else if (a is IEquatable<T>)
                {
                    return (a as IEquatable<T>).Equals(b);
                }
                else if (a is IComparable<T>)
                {
                    return (a as IComparable<T>).CompareTo(b) == 0;
                }
                else if (a is IComparable)
                {
                    return (a as IComparable).CompareTo(b) == 0;
                }
                else if (object.ReferenceEquals(a, b))
                {
                    return true;
                }
                else
                {
                    return a.Equals(b);
                }
            }
            /// <summary>
            /// Greaters the specified a.
            /// </summary>
            /// <param name="a">a.</param>
            /// <param name="b">The b.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            static bool Greater(T a, T b)
            {
                bool a_ = (object)a == null;
                bool b_ = (object)b == null;

                if (a_ && b_) { return false; }
                else if (a_) { return false; }
                else if (a is IComparable<T>)
                {
                    return (a as IComparable<T>).CompareTo(b) > 0;
                }
                else if (a is IComparable)
                {
                    return (a as IComparable).CompareTo(b) > 0;
                }
                else { return false; }
            }
            /// <summary>
            /// Lesses the specified a.
            /// </summary>
            /// <param name="a">a.</param>
            /// <param name="b">The b.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            static bool Less(T a, T b)
            {
                bool a_ = (object)a == null;
                bool b_ = (object)b == null;

                if (a_ && b_) { return false; }
                else if (a_) { return true; }
                else if (a is IComparable<T>)
                {
                    return (a as IComparable<T>).CompareTo(b) < 0;
                }
                else if (a is IComparable)
                {
                    return (a as IComparable).CompareTo(b) < 0;
                }
                else { return false; }
            }
            /// <summary>
            /// Occurses the specified a.
            /// </summary>
            /// <param name="a">a.</param>
            /// <param name="b">The b.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            static bool Occurs(T a, T b)
            {
                bool a_ = (object)a == null;
                bool b_ = (object)b == null;

                if (a_ || b_) { return false; }
                return a.ToString().IndexOf(b.ToString()) != -1;
            }
            /// <summary>
            /// Occurses the ignore case.
            /// </summary>
            /// <param name="a">a.</param>
            /// <param name="b">The b.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            static bool OccursIgnoreCase(T a, T b)
            {
                bool a_ = (object)a == null;
                bool b_ = (object)b == null;

                if (a_ || b_) { return false; }
                return a.ToString().ToUpper().IndexOf(b.ToString().ToUpper()) != -1;
            }
            /// <summary>
            /// Beginses the specified a.
            /// </summary>
            /// <param name="a">a.</param>
            /// <param name="b">The b.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            static bool Begins(T a, T b)
            {
                bool a_ = (object)a == null;
                bool b_ = (object)b == null;

                if (a_ || b_) { return false; }
                return a.ToString().StartsWith(b.ToString());
            }
            /// <summary>
            /// Beginses the ignore case.
            /// </summary>
            /// <param name="a">a.</param>
            /// <param name="b">The b.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            static bool BeginsIgnoreCase(T a, T b)
            {
                bool a_ = (object)a == null;
                bool b_ = (object)b == null;

                if (a_ || b_) { return false; }
                return a.ToString().ToUpper().StartsWith(b.ToString().ToUpper());
            }
            /// <summary>
            /// Endses the specified a.
            /// </summary>
            /// <param name="a">a.</param>
            /// <param name="b">The b.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            static bool Ends(T a, T b)
            {
                bool a_ = (object)a == null;
                bool b_ = (object)b == null;

                if (a_ || b_) { return false; }
                return a.ToString().EndsWith(b.ToString());
            }
            /// <summary>
            /// Endses the ignore case.
            /// </summary>
            /// <param name="a">a.</param>
            /// <param name="b">The b.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            static bool EndsIgnoreCase(T a, T b)
            {
                bool a_ = (object)a == null;
                bool b_ = (object)b == null;

                if (a_ || b_) { return false; }
                return a.ToString().ToUpper().EndsWith(b.ToString().ToUpper());
            }
            /// <summary>
            /// Strings the equal.
            /// </summary>
            /// <param name="a">a.</param>
            /// <param name="b">The b.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            static bool StrEqual(T a, T b)
            {
                bool a_ = (object)a == null;
                bool b_ = (object)b == null;

                if (a_ || b_) { return false; }
                return a.ToString() == (b.ToString());
            }
            /// <summary>
            /// Strings the equal ignore case.
            /// </summary>
            /// <param name="a">a.</param>
            /// <param name="b">The b.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            static bool StrEqualIgnoreCase(T a, T b)
            {
                bool a_ = (object)a == null;
                bool b_ = (object)b == null;

                if (a_ || b_) { return false; }
                return a.ToString().ToUpper() == (b.ToString().ToUpper());
            }
            /// <summary>
            /// Strings the greater.
            /// </summary>
            /// <param name="a">a.</param>
            /// <param name="b">The b.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            static bool StrGreater(T a, T b)
            {
                bool a_ = (object)a == null;
                bool b_ = (object)b == null;

                if (a_ && b_) { return false; }
                else if (a_) { return false; }
                else if (b_) { return true; }
                else
                {
                    return NumericStringComparer.Compare(a.ToString(), b.ToString()) > 0;
                }
            }
            /// <summary>
            /// Strings the less.
            /// </summary>
            /// <param name="a">a.</param>
            /// <param name="b">The b.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            static bool StrLess(T a, T b)
            {
                bool a_ = (object)a == null;
                bool b_ = (object)b == null;

                if (a_ && b_) { return false; }
                else if (b_) { return false; }
                else if (a_) { return true; }
                else
                {
                    return NumericStringComparer.Compare(a.ToString(), b.ToString()) < 0;
                }
            }
            #endregion

            #region PUBLIC METHODS
            /// <summary>
            /// Compares the specified left.
            /// </summary>
            /// <param name="left">The left.</param>
            /// <param name="right">The right.</param>
            /// <param name="criteria">The criteria.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            public static bool Compare(T left, T right, Criteria criteria)
            {
                int val = (int)criteria;
                int id = (val < 0) ? -val - 1 : val;

                if (id <= 12)
                {
                    if (ctasks[id] == 0) { CreateOperator(criteria, id); }
                    if (ctasks[id] == Status.Supported)
                    {
                        try
                        {
                            if (val < 0)
                            {
                                return !comparison[id](left, right);
                            }
                            else
                            {
                                var func = comparison[id];
                                return func(left, right);
                            }
                        }
                        catch {; }
                    }
                }
                return false;
            }

            /// <summary>
            /// Compares the range.
            /// </summary>
            /// <param name="left">The left.</param>
            /// <param name="criteria">The criteria.</param>
            /// <param name="value1">The value1.</param>
            /// <param name="value2">The value2.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            public static bool CompareRange(T left, MultCriteria criteria, T value1, T value2)
            {
                if (criteria == MultCriteria.Between)
                {
                    return Compare(left, value1, Criteria.NotLessThan) &&
                        Compare(left, value2, Criteria.NotGreaterThan);
                }
                else if (criteria == MultCriteria.NotBetween)
                {
                    return Compare(left, value1, Criteria.LessThan) &&
                        Compare(left, value2, Criteria.GreaterThan);
                }
                return false;
            }

            /// <summary>
            /// Compares the range.
            /// </summary>
            /// <param name="left">The left.</param>
            /// <param name="criteria">The criteria.</param>
            /// <param name="value1">The value1.</param>
            /// <param name="value2">The value2.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            public static bool CompareRange(T left, MultCriteria criteria, object value1, object value2)
            {
                if (criteria == MultCriteria.Between)
                {
                    return Compare(left, value1, Criteria.NotLessThan) &&
                        Compare(left, value2, Criteria.NotGreaterThan);
                }
                else if (criteria == MultCriteria.NotBetween)
                {
                    return Compare(left, value1, Criteria.LessThan) &&
                        Compare(left, value2, Criteria.GreaterThan);
                }
                return false;
            }

            /// <summary>
            /// Compares the specified left.
            /// </summary>
            /// <param name="left">The left.</param>
            /// <param name="right">The right.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            public static bool Compare(T left, T right)
            {
                return Compare(left, right, Criteria.Equal);
            }

            /// <summary>
            /// Compares the specified left.
            /// </summary>
            /// <param name="left">The left.</param>
            /// <param name="right">The right.</param>
            /// <param name="criteria">The criteria.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            public static bool Compare(T left, object right, Criteria criteria)
            {
                if (right != null)
                {
                    switch (criteria)
                    {
                        case Criteria.Occurs:
                        case Criteria.BeginsWith:
                        case Criteria.EndsWith:
                        case Criteria.OccursNoCase:
                        case Criteria.BeginsWithNoCase:
                        case Criteria.EndsWithNoCase:
                        case Criteria.StringEqual:
                        case Criteria.StringEqualNoCase:
                        case Criteria.StringNumGreaterThan:
                        case Criteria.StringNumLessThan:
                        case Criteria.NotOccurs:
                        case Criteria.NotBeginsWith:
                        case Criteria.NotEndsWith:
                        case Criteria.NotOccursNoCase:
                        case Criteria.NotBeginsWithNoCase:
                        case Criteria.NotEndsWithNoCase:
                        case Criteria.NotStrEqual:
                        case Criteria.NotStrEqualNoCase:
                        case Criteria.NotStringGreaterThan:
                        case Criteria.NotStringLessThan:
                            return Operator<string>.Compare(left.ToString(), right.ToString(), criteria);
                        default:
                            if(right is T)
                            {
                                return Compare(left, (T)right, criteria);
                            }
                            break;
                    }
                }
                return false;
            }
            #endregion

            /// <summary>
            /// Class StringNumericComparer. This class cannot be inherited.
            /// </summary>
            /// <seealso cref="System.Collections.Generic.IComparer{System.String}" />
            sealed class NumericStringComparer : IComparer<string>
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="NumericStringComparer"/> class.
                /// </summary>
                public NumericStringComparer() { }

                /// <summary>
                /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
                /// </summary>
                /// <param name="x">The first object to compare.</param>
                /// <param name="y">The second object to compare.</param>
                /// <returns>A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.</returns>
                int IComparer<string>.Compare(string x, string y)
                {
                    return Compare(x, y);
                }

                /// <summary>
                /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
                /// </summary>
                /// <param name="x">The first object to compare.</param>
                /// <param name="y">The second object to compare.</param>
                /// <returns>A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.</returns>
                public static int Compare(string x, string y)
                {
                    //get rid of special cases
                    if ((x == null) && (y == null)) return 0;
                    else if (x == null) return -1;
                    else if (y == null) return 1;

                    if ((x.Equals(String.Empty) && (y.Equals(String.Empty)))) return 0;
                    else if (x.Equals(String.Empty)) return -1;
                    else if (y.Equals(String.Empty)) return -1;

                    //WE style, special case
                    bool sp1 = Char.IsLetterOrDigit(x, 0);
                    bool sp2 = Char.IsLetterOrDigit(y, 0);
                    if (sp1 && !sp2) return 1;
                    if (!sp1 && sp2) return -1;

                    int i1 = 0, i2 = 0; //current index
                    int r = 0; // temp result
                    while (true)
                    {
                        bool c1 = Char.IsDigit(x, i1);
                        bool c2 = Char.IsDigit(y, i2);
                        if (!c1 && !c2)
                        {
                            bool letter1 = Char.IsLetter(x, i1);
                            bool letter2 = Char.IsLetter(y, i2);
                            if ((letter1 && letter2) || (!letter1 && !letter2))
                            {
                                if (letter1 && letter2)
                                {
                                    r = Char.ToLower(x[i1]).CompareTo(Char.ToLower(y[i2]));
                                }
                                else
                                {
                                    r = x[i1].CompareTo(y[i2]);
                                }
                                if (r != 0) return r;
                            }
                            else if (!letter1 && letter2) return -1;
                            else if (letter1 && !letter2) return 1;
                        }
                        else if (c1 && c2)
                        {
                            r = CompareNum(x, ref i1, y, ref i2);
                            if (r != 0) return r;
                        }
                        else if (c1)
                        {
                            return -1;
                        }
                        else if (c2)
                        {
                            return 1;
                        }
                        i1++;
                        i2++;
                        if ((i1 >= x.Length) && (i2 >= y.Length))
                        {
                            return 0;
                        }
                        else if (i1 >= x.Length)
                        {
                            return -1;
                        }
                        else if (i2 >= y.Length)
                        {
                            return -1;
                        }
                    }
                }

                /// <summary>
                /// Compares the number.
                /// </summary>
                /// <param name="s1">The s1.</param>
                /// <param name="i1">The i1.</param>
                /// <param name="s2">The s2.</param>
                /// <param name="i2">The i2.</param>
                /// <returns>System.Int32.</returns>
                private static int CompareNum(string s1, ref int i1, string s2, ref int i2)
                {
                    int nzStart1 = i1, nzStart2 = i2; // nz = non zero
                    int end1 = i1, end2 = i2;

                    ScanNumEnd(s1, i1, ref end1, ref nzStart1);
                    ScanNumEnd(s2, i2, ref end2, ref nzStart2);
                    int start1 = i1; i1 = end1 - 1;
                    int start2 = i2; i2 = end2 - 1;

                    int nzLength1 = end1 - nzStart1;
                    int nzLength2 = end2 - nzStart2;

                    if (nzLength1 < nzLength2) return -1;
                    else if (nzLength1 > nzLength2) return 1;

                    for (int j1 = nzStart1, j2 = nzStart2; j1 <= i1; j1++, j2++)
                    {
                        int r = s1[j1].CompareTo(s2[j2]);
                        if (r != 0) return r;
                    }
                    // the nz parts are equal
                    int length1 = end1 - start1;
                    int length2 = end2 - start2;
                    if (length1 == length2) return 0;
                    if (length1 > length2) return -1;
                    return 1;
                }

                /// <summary>
                /// Scans the number end.
                /// </summary>
                /// <param name="s">The s.</param>
                /// <param name="start">The start.</param>
                /// <param name="end">The end.</param>
                /// <param name="nzStart">The nz start.</param>
                private static void ScanNumEnd(string s, int start, ref int end, ref int nzStart)
                {
                    nzStart = start;
                    end = start;
                    bool countZeros = true;
                    while (Char.IsDigit(s, end))
                    {
                        if (countZeros && s[end].Equals('0'))
                        {
                            nzStart++;
                        }
                        else countZeros = false;
                        end++;
                        if (end >= s.Length) break;
                    }
                }
            }
        }
        #endregion
    }
}
#endif
//+:cnd:noEmit
