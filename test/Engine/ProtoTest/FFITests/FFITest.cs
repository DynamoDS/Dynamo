using System.Linq;
using FFITarget;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Mirror;
namespace ProtoTest.TD.FFI
{
    class FFITest : ProtoTestBase
    {
        [Test]
        public void T020_Sample_Test()
        {
            string code = @"
import (""FFITarget.dll"");
	vec =  ClassFunctionality.ClassFunctionality(3,4,0); 
	o = vec.IntVal;
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("o", 7, 0);
        }

        [Test]
        public void T021_Vector_ByCoordinates_1458422_Regress()
        {
            string code = @"
import (""FFITarget.dll"");
    cf = ClassFunctionality.ClassFunctionality(0,1,2);
    intVal = cf.IntVal;
    vc1 = cf.ClassProperty;
    vc2 = cf.ClassProperty;
    vc1Value = vc1.SomeValue;
    vc2Value = vc2.SomeValue;
    vcValueEquality = vc1Value == vc2Value;
 
	
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("vcValueEquality", true);

            thisTest.Verify("intVal", 3);
            thisTest.Verify("vc1Value", 3);
            thisTest.Verify("vc2Value", 3);

}

        [Test]
        [Category("SmokeTest")]
        public void T022_Array_Marshal()
        {
            string code = @"
import (Dummy from ""FFITarget.dll"");
dummy = Dummy.Dummy();
arr = [0.0,1.0,2.0,3.0,4.0,5.0,6.0,7.0,8.0,9.0,10.0];
sum_1_10 = dummy.SumAll1D(arr);
twice_arr = dummy.Twice(arr);
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sum_1_10", 55.0, 0);
            object[] Expectedresult = { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 };
            object[] Expectedresult2 = { 0.0, 2.0, 4.0, 6.0, 8.0, 10.0, 12.0, 14.0, 16.0, 18.0, 20.0 };
            thisTest.Verify("twice_arr", Expectedresult2, 0);
        }
    

        [Test]
        public void T023_MethodOverload()
        {
            string code = @"
import(""FFITarget.dll"");
cf1 = ClassFunctionality.ClassFunctionality(1);
cf2 = ClassFunctionality.ClassFunctionality(2);
i = 3;

o1 = cf1.OverloadedAdd(cf2);
o2 = cf1.OverloadedAdd(i);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("o1", 3);
            thisTest.Verify("o2", 4);


        }

        [Test]
        public void T024_MethodOverload_static()
        {
            string code = @"
import(""FFITarget.dll"");
cf1 = ClassFunctionality.ClassFunctionality(1);
dp1 = DummyPoint.ByCoordinates(0,1,2);

o1 = OverloadTarget.StaticOverload(1);
o2 = OverloadTarget.StaticOverload(cf1);
o3 = OverloadTarget.StaticOverload(dp1);


";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("o1", 0);
            thisTest.Verify("o2", 1);
            thisTest.Verify("o3", 2);


        }

        [Test]
        public void T025_MethodOverload_DifferentPrimitiveType()
        {
            string code = @"
import(""FFITarget.dll"");

o1 = OverloadTarget.DifferentPrimitiveType(1);
o2 = OverloadTarget.DifferentPrimitiveType(true);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("o1", 2);
            thisTest.Verify("o2", 1);
        }

        [Test]
        public void T026_MethodOverload_DifferentIEnumerable()
        {
            string code = @"
import(""FFITarget.dll"");

a = DummyClassA.DummyClassA();
o1 = OverloadTarget.IEnumerableOfDifferentObjectType(a);
o2 = OverloadTarget.IEnumerableOfDifferentObjectType(a);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("o1", 3);
            thisTest.Verify("o2", 3);

        }

        [Test]
        public void MethodWithRefOutParams_NoLoad()
        {
            string code = @"
import(""FFITarget.dll"");
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            string ffiTargetClass = "ClassWithRefParams";

            // Assert that the class name is indeed a class
            ClassMirror type = null;
            Assert.DoesNotThrow(() => type = new ClassMirror(ffiTargetClass, thisTest.GetTestCore()));

            var members = type.GetMembers();

            var expected = new string[] { "ClassWithRefParams" };

            var actual = members.OrderBy(n => n.Name).Select(x => x.Name).ToArray();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestDefaultArgumentAttribute()
        {
            string code = @"
import (TestData from ""FFITarget.dll"");
";
            thisTest.RunScriptSource(code);
            var core = thisTest.GetTestCore();
            var testDataClassIndex = core.ClassTable.IndexOf("TestData");
            var testDataClass = core.ClassTable.ClassNodes[testDataClassIndex];
            var funcNode = testDataClass.ProcTable.GetFunctionsByName("GetCircleArea")
                                                  .Where(p => p.IsStatic)
                                                  .FirstOrDefault();
            var argument = funcNode.ArgumentInfos.First();

            Assert.IsNotNull(argument);
            Assert.IsNotNull(argument.Attributes);

            object o;
            Assert.IsTrue(argument.Attributes.TryGetAttribute("DefaultArgumentAttribute", out o));

            string expression = o as string;
            Assert.IsTrue(expression.Equals("TestData.GetFloat()"));
        }

        [Test]
        public void TestToStringDoesNotThrow()
        {
            string code = @"
import (ClassWithExceptionToString from ""FFITarget.dll"");
x = ClassWithExceptionToString.Construct();
";
            var mirror = thisTest.RunScriptSource(code);
            var x = mirror.GetValue("x");
            Assert.DoesNotThrow(() => { mirror.GetStringValue(x,this.core.Heap, 0); }); 
            
        }

        [Test]
        public void TestReturnArbitraryDimensionAttribute()
        {
            string code = @"
import (TestCSharpAttribute from ""FFITarget.dll"");
x = TestCSharpAttribute.TestReturnAttribute();
";
            var mirror = thisTest.RunScriptSource(code);
            var x = mirror.GetValue("x");
            thisTest.Verify("x", new object[] { 1.3, new double[] { 4.5, 7.8 } });

        }
#if NETFRAMEWORK
        [Test]
        public void Test_Embedded_Interop_Types()
        {
            string code = @"
import (ClassFunctionality from ""FFITarget.dll"");
import (EmbeddedInteropTestClass from ""..\\..\\..\\test\\test_dependencies\\EmbeddedInterop.dll"");

  val = EmbeddedInteropTestClass.GetExcelInteropType();
  o = ClassFunctionality.TestExcelInteropType(val);
";

            thisTest.RunScriptSource(code);
            thisTest.Verify("o", true);
        }
#endif
    }
}
