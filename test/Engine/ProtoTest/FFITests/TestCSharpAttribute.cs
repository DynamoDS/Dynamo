using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Mirror;
using ProtoFFITests;
using ProtoTestFx.TD;
using System.Linq;
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
            var core =execMirror.MirrorTarget.Core;

            ClassMirror classMirror = new ClassMirror("TestCSharpAttribute", core);
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
    }
}
