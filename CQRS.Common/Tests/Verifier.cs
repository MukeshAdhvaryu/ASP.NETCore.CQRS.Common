/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/
//-:cnd:noEmit
#if MODEL_ADDTEST
//+:cnd:noEmit

//-:cnd:noEmit
#if MODEL_USEXUNIT
    using Assert = Xunit.Assert;
#elif MODEL_USENUNIT
    using Assert = NUnit.Framework.Assert;
#else
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
#endif
//+:cnd:noEmit


using System.Collections;

namespace CQRS.Common.Tests
{
    public static class Verifier
    {
        #region EQUAL
        public static void Equal<T>(T expected, T actual)
        {
            //-:cnd:noEmit
#if MODEL_USEXUNIT
            Assert.Equal(expected, actual);
            return;
#elif MODEL_USENUNIT
            Assert.AreEqual(expected, actual);
            return;
#else
            Assert.AreEqual(expected, actual);
            return;
#endif
            //+:cnd:noEmit
        }
        #endregion

        #region EQUAL
        public static void NotEqual<T>(T expected, T actual)
        {
            //-:cnd:noEmit
#if MODEL_USEXUNIT
            Assert.NotEqual(expected, actual);
            return;
#elif MODEL_USENUNIT
            Assert.AreNotEqual(expected, actual);
            return;
#else
            Assert.AreNotEqual(expected, actual);
            return;
#endif
            //+:cnd:noEmit
        }
        #endregion

        #region EMPTY
        public static void Empty(IEnumerable expected)
        {
            //-:cnd:noEmit
#if MODEL_USEXUNIT
            Assert.Empty(expected);
            return;
#elif MODEL_USENUNIT
            Assert.IsEmpty(((IEnumerable)expected));
            return;
#else
            Assert.IsFalse(((IEnumerable)expected).GetEnumerator().MoveNext());
            return;
#endif
            //+:cnd:noEmit
        }

        public static void Empty(string expected)
        {
            //-:cnd:noEmit
#if MODEL_USEXUNIT
            Assert.Empty(expected);
            return;
#elif MODEL_USENUNIT
            Assert.IsEmpty(expected);
            return;
#else
            Assert.IsTrue(string.IsNullOrEmpty((string)(object)expected));
            return;
#endif
            //+:cnd:noEmit
        }
        #endregion

        #region IS NULL
        public static void IsNull<T>(T expected)
        {
            //-:cnd:noEmit
#if MODEL_USEXUNIT
            Assert.Null(expected);
            return;
#elif MODEL_USENUNIT
            Assert.IsNull(expected);
            return;
#else
            Assert.IsNull(expected);
            return;
#endif
            //+:cnd:noEmit
        }
        #endregion

        #region NOT NULL
        public static void NotNull<T>(T expected)
        {
            //-:cnd:noEmit
#if MODEL_USEXUNIT
            Assert.NotNull(expected);
            return;
#elif MODEL_USENUNIT
            Assert.NotNull(expected);
            return;
#else
            Assert.IsNotNull(expected);
            return;
#endif
            //+:cnd:noEmit
        }
        #endregion

        #region IsType
        public static void IsOfType<T>(object expected)
        {
            //-:cnd:noEmit
#if MODEL_USEXUNIT
            Assert.IsType<T>(expected);
            return;
#elif MODEL_USENUNIT
            Assert.IsInstanceOf<T>(expected);
            return;
#else
            Assert.IsInstanceOfType(expected, typeof(T));
            return;
#endif
            //+:cnd:noEmit
        }
        #endregion
    }
}
//-:cnd:noEmit
#endif
//+:cnd:noEmit
