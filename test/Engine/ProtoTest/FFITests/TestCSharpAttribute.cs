using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProtoCore.Mirror;
using ProtoFFI;

namespace ProtoTest.FFITests
{
    [TestFixture]
    class TestCSharpAttribute : ProtoTestBase 
    {
        [Test]
        public void BasicClassAttributeTest()
        {
            var assembly = System.Reflection.Assembly.UnsafeLoadFrom("FFITarget.dll");
            var testClass = assembly.GetType("FFITarget.TestCSharpAttribute");
            var testMethod = testClass.GetMethod("Test");

            String code = @"import(""FFITarget.dll"");";
            var execMirror = thisTest.RunScriptSource(code);
            var staticCore = thisTest.GetTestCore();

            ClassMirror classMirror = new ClassMirror("TestCSharpAttribute", staticCore);
            Assert.IsNotNull(classMirror);
            var classAttributes = classMirror.GetClassAttributes();
            Assert.IsNotNull(classAttributes);
            var ffiClassAttribute = classAttributes as FFIClassAttributes;
            Assert.IsNotNull(ffiClassAttribute);
            var classCustomAttributes = ffiClassAttribute.Attributes;
            Assert.AreEqual(2, classCustomAttributes.Count());
            Assert.IsTrue(classCustomAttributes.SequenceEqual(testClass.GetCustomAttributes(false)));

            MethodMirror methodMirror = classMirror.GetFunctions().FirstOrDefault(m => m.MethodName.Equals("Test"));
            var methodAttributes = methodMirror.GetMethodAttributes();
            Assert.IsNotNull(methodAttributes);
            var ffiMethodAttributes = methodAttributes as FFIMethodAttributes;
            Assert.IsNotNull(ffiMethodAttributes);
            var methodCustomAttributes = ffiMethodAttributes.Attributes;
            Assert.AreEqual(2, methodCustomAttributes.Count());
            Assert.IsTrue(methodCustomAttributes.SequenceEqual(testMethod.GetCustomAttributes(false)));
        }

        [Test]
        public void AttributeUtilsTest()
        {
            var type = typeof(Attributes.CantImportInVM);
            Assert.IsTrue(CheckAttributes(type, AttributeUtils.SupressImportIntoVM), "Can't validate SupressImportIntoVMAttribute");

            var invisible = type.GetProperty("InvisibleProperty");
            Assert.IsTrue(CheckAttributes(invisible, AttributeUtils.HiddenInDynamoLibrary), "Can't validate IsVisibleInDynamoLibraryAttribute");

            var visible = type.GetMethod("VisibleMethod");
            Assert.IsFalse(CheckAttributes(visible, AttributeUtils.HiddenInDynamoLibrary), "Can't validate IsVisibleInDynamoLibraryAttribute");

            var simple = type.GetMethod("SimpleMethod");
            Assert.IsFalse(CheckAttributes(simple, AttributeUtils.SupressImportIntoVM), "SimpleMethod has SupressImportIntoVMAttribute");
            Assert.IsFalse(CheckAttributes(simple, AttributeUtils.HiddenInDynamoLibrary), "SimpleMethod is Hidden in library");
        }

        private bool CheckAttributes(MemberInfo m, Func<Attribute, bool> check)
        {
            var attributes = m.GetCustomAttributes();
            foreach (Attribute item in attributes)
            {
                if (check(item)) return true;
            }

            return false;
        }
    }

    namespace Attributes
    {
        [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
        public sealed class SupressImportIntoVMAttribute : Attribute
        {
        }

        /// <summary>
        /// This attribute is used to specify whether the item will be displayed
        /// in the library.
        /// </summary>
        [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
        public sealed class IsVisibleInDynamoLibraryAttribute : Attribute
        {
            public IsVisibleInDynamoLibraryAttribute(bool visible)
            {
                Visible = visible;
            }

            public bool Visible { get; private set; }
        }

        [SupressImportIntoVM]
        public class CantImportInVM
        {
            [IsVisibleInDynamoLibrary(false)]
            public bool InvisibleProperty { get; set; }

            [IsVisibleInDynamoLibrary(true)]
            public void VisibleMethod() { }

            public void SimpleMethod() { }
        }
    }
}
