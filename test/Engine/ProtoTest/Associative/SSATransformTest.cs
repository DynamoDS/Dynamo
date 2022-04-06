using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.Associative
{
    class SSATransformTest : ProtoTestBase
    {
        [Test]
        [Category("DSDefinedClass_Ported")]
        public void UpdateMember01()
        {
            String code =
@"
import(""FFITarget.dll"");
p = ClassFunctionality.ClassFunctionality(1);
a = p.IntVal;
p.IntVal = 10;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 10);
        }

        [Test]
        public void ArrayAssignmentNoCycle1()
        {
            String code =
@"
// Script must not cycle
a=[0,1,2];
x=[10,11,12];
a[0] = x[0];
x[1] = a[1];
y = x[1]; // 1
";
            thisTest.RunAndVerifyBuildWarning(code, ProtoCore.BuildData.WarningID.InvalidStaticCyclicDependency);
        }

        [Test]
        public void ArrayAssignmentNoCycle2()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4117
            String code =
@"
// Script must not cycle
a=[0,1,2];
x=[10,11,12];
i = 1;
a[0] = x[0];
x[i] = a[i];
y = x[i]; // 1
";
            thisTest.RunAndVerifyBuildWarning(code, ProtoCore.BuildData.WarningID.InvalidStaticCyclicDependency);
        }

        [Test]
        public void TestReplicationGuide01()
        {
            String code =
@"
a = [1,2];
b = [1,2];
c = a<1> + b<2>;
x = c[0];
y = c[1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 2, 3 });
            thisTest.Verify("y", new object[] { 3, 4 });
        }

        [Test]
        public void TestReplicationGuide02()
        {
            String code =
@"
a = [1,2];
b = [1,2];
a = a<1> + b<2>;
x = a[0];
y = a[1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 2, 3 });
            thisTest.Verify("y", new object[] { 3, 4 });
        }

        [Test]
        public void TestReplicationGuideOnFunction01()
        {
            String code =
@"
def f()
{
    return = [ 1, 2 ];
}
def g()
{
    return = [ 3, 4 ];
}
x = f()<1> + g()<2>;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new Object[] { new object[] { 4, 5 }, new object[] { 5, 6 } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestArrayIndexingFromFunction01()
        {
            String code =
@"
def foo()
{
    return = [1, 2, 3];
}
x = foo()[0];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
        }

        [Test]
        public void TestRecursiveAssociativeImperativeCondition01()
        {
            String code =
@"
def f(x : int)
{
    loc = [Imperative]
    {
        if (x > 1)
        {
            return = f(x - 1) + x;
        }
        return = x;
    } 
    return = loc;
}
y = f(10);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", 55);
        }
    }
}