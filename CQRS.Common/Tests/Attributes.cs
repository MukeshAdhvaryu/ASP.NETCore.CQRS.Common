/* Licensed under the MIT/X11 license.
* This notice may not be removed from any source distribution.
 Author: Mukesh Adhvaryu.
*/
//-:cnd:noEmit
#if MODEL_ADDTEST
//+:cnd:noEmit

using System.ComponentModel;
using System.Reflection;

using Castle.Core.Resource;


//-:cnd:noEmit
#if MODEL_USEXUNIT
using Xunit;
using Xunit.Sdk;
#elif MODEL_USENUNIT
using NUnit.Framework;
using NUnit.Framework.Internal;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
//+:cnd:noEmit

namespace CQRS.Common.Tests.Attributes
{
    //-:cnd:noEmit
#if MODEL_USEXUNIT
    #region XUNIT
    public class TestableAttribute : Attribute
    { }
    public class NoArgsAttribute : FactAttribute
    { }
    public class WithArgsAttribute : TheoryAttribute
    { }

    [DataDiscoverer("Xunit.Sdk.InlineDataDiscoverer", "xunit.core")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ArgsAttribute : DataAttribute
    {
        readonly object[] data;

        public ArgsAttribute(params object[] _data)
        {
            data = _data;
        }
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            return new[] { data };
        }
    }

    /// <summary>
    /// Skips the method from being tested.
    /// test method.
    /// </summary>
    [XunitTestCaseDiscoverer("Xunit.Sdk.FactDiscoverer", "xunit.execution.{Platform}")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NoArgsSkipAttribute : FactAttribute
    {
        public NoArgsSkipAttribute()
        {
            base.Skip = "Method is skipped from testing!";
        }

        public NoArgsSkipAttribute(string skip)
        {
            base.Skip = skip ?? "Method is skipped from testing!";
        }
    }

    /// <summary>
    /// Skips the method from being tested.
    /// test method.
    /// </summary>
    [XunitTestCaseDiscoverer("Xunit.Sdk.TheoryDiscoverer", "xunit.execution.{Platform}")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class WithArgsSkipAttribute : TheoryAttribute
    {
        public WithArgsSkipAttribute()
        {
            base.Skip = "Method is skipped from testing!";
        }

        public WithArgsSkipAttribute(string skip)
        {
            base.Skip = skip ?? "Method is skipped from testing!";
        }
    }

    /// <summary>
    /// Indicates the source to be used to provide arguments for a test method which uses this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ArgSourceAttribute<T> : ClassDataAttribute where T : ArgSource, IEnumerable<object[]>, new()
    {
        static T Instance = new T();

        public ArgSourceAttribute() :
            base(typeof(T))
        { }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            return Instance.Data;
        }
    }

    /// <summary>
    /// Provides a data source for a data theory, with the data coming from a static field, property or method.
    /// The member must return something compatible with IEnumerable&lt;object[]&gt;  
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ArgSourceAttribute : MemberDataAttributeBase
    {
        public ArgSourceAttribute(string memberName, params object[] parameters) :
            base(memberName, parameters)
        {
        }

        public ArgSourceAttribute(Type sourceDeclaringType, string memberName, params object[] parameters) :
            this(memberName, parameters)
        {
            MemberType = sourceDeclaringType;
        }

        protected override object[] ConvertDataItem(MethodInfo testMethod, object item)
        {
            if (item == null)
                return null;

            var array = item as object[];
            if (array == null)
                throw new ArgumentException($"Property {MemberName} on {MemberType ?? testMethod.DeclaringType} yielded an item that is not an object[]");

            return array;
        }
    }
    #endregion
#elif MODEL_USENUNIT
    #region NUNIT
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class TestableAttribute : TestFixtureAttribute 
    { }

    public class NoArgsAttribute : TestAttribute
    { }
    public class WithArgsAttribute : TestAttribute 
    { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ArgsAttribute : TestCaseAttribute
    {
        public ArgsAttribute(params object?[]? arguments) :
            base(arguments)
        { }

        public ArgsAttribute(object? arg) :
            base(arg)
        { }

        public ArgsAttribute(object? arg1, object? arg2) :
            base(arg1, arg2)
        { }

        public ArgsAttribute(object? arg1, object? arg2, object? arg3) :
            base(arg1, arg2, arg3)
        { }
    }

    /// <summary>
    /// Skips the method from being tested.
    /// test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NoArgsSkipAttribute : IgnoreAttribute
    {
        public NoArgsSkipAttribute() :
            base("Method is skipped from testing!")
        { }

        public NoArgsSkipAttribute(string skip) : base(skip ?? "Method is skipped from testing!")
        { }
    }

    /// <summary>
    /// Skips the method from being tested.
    /// test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class WithArgsSkipAttribute : IgnoreAttribute
    {
        public WithArgsSkipAttribute() :
            base("Method is skipped from testing!")
        { }

        public WithArgsSkipAttribute(string skip) : base(skip ?? "Method is skipped from testing!")
        { }
    }

    /// <summary>
    /// Indicates the source to be used to provide arguments for a test method which uses this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ArgSourceAttribute<T> : TestCaseSourceAttribute where T: ArgSource, IEnumerable<object[]>, new()
    {
        public ArgSourceAttribute() :
            base(typeof(T))
        { }
    }

    /// <summary>
    /// Provides a data source for a data theory, with the data coming from a static field, property or method.
    /// The member must return something compatible with IEnumerable&lt;object[]&gt;  
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ArgSourceAttribute : TestCaseSourceAttribute
    {
        public ArgSourceAttribute(string memberName) :
            base(memberName)
        {
        }
        public ArgSourceAttribute(Type sourceDeclaringType, string memberName) :
          base(sourceDeclaringType, memberName)
        {
        }
        public ArgSourceAttribute(Type sourceDeclaringType, string memberName, object[] parameters) :
            base(sourceDeclaringType, memberName, parameters)
        {
        }
        public ArgSourceAttribute(string memberName, object[] parameters) :
            base(memberName, parameters)
        {
        }
    }
    #endregion
#else
    #region MSTEST
    public class TestableAttribute : TestClassAttribute 
    { }

    public class NoArgsAttribute : TestMethodAttribute
    { }

    public class WithArgsAttribute : TestMethodAttribute
    { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ArgsAttribute : DataRowAttribute
    {
        public ArgsAttribute() :
            base()
        { }

        public ArgsAttribute(object data1) :
            base(data1)
        { }

        public ArgsAttribute(object data1, params object[] moreData) :
            base(data1, moreData)
        { }
    }


    /// <summary>
    /// Skips the method from being tested.
    /// test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class NoArgsSkipAttribute : Attribute
    {
        public NoArgsSkipAttribute() 
        { }

        public NoArgsSkipAttribute(string skip)
        { } 
    }


    /// <summary>
    /// Skips the method from being tested.
    /// test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class WithArgsSkipAttribute : Attribute
    {
        public WithArgsSkipAttribute() 
        { }

        public WithArgsSkipAttribute(string skip) 
        { }
    }


    /// <summary>
    /// Indicates the source to be used to provide arguments for a test method which uses this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ArgSourceAttribute<T> : Attribute, ITestDataSource where T: ArgSource, IEnumerable<object[]>, new()
    {
        static T Instance = new T();
        public IEnumerable<object[]> GetData(MethodInfo methodInfo)
        {
            var data = Instance.Data;
            foreach (var item in data)
                yield return item;
        }

        public string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            var info = methodInfo.GetParameters().Select((p, i) => p.Name + ": " + (i < data.Length ? data[i].ToString() : "not provided"));
            return methodInfo.Name + string.Format("({0})", string.Join(",", info));
        }
    }

    /// <summary>
    /// Provides a data source for a data theory, with the data coming from a static field, property or method.
    /// The member must return something compatible with IEnumerable&lt;object[]&gt;  
    /// </summary>
    public class ArgSourceAttribute : Attribute, ITestDataSource
    {
        DynamicDataAttribute dataAttribute;

        public ArgSourceAttribute(string sourceName, MemberType sourceType = MemberType.Property)
        {
            dataAttribute = new DynamicDataAttribute(sourceName, (DynamicDataSourceType)sourceType);
        }

        public ArgSourceAttribute(Type sourceDeclaringType, string sourceName, MemberType sourceType = MemberType.Property) 
        {
            dataAttribute = new DynamicDataAttribute(sourceName, sourceDeclaringType, (DynamicDataSourceType)sourceType);
        }

        public IEnumerable<object[]> GetData(MethodInfo methodInfo)
        {
            return dataAttribute.GetData(methodInfo);
        }

        public string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            var info = methodInfo.GetParameters().Select((p, i) => p.Name + ": " + (i < data.Length ? data[i].ToString() : "not provided"));
            return methodInfo.Name + string.Format("({0})", string.Join(",", info));
        }
    }
    #endregion
#endif
    //+:cnd:noEmit
}
//-:cnd:noEmit
#endif
//+:cnd:noEmit

