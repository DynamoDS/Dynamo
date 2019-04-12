using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.DebugTests
{
    [TestFixture]
    class SetValueTests : ProtoTestBase
    {
        [Test]
        public void MinimalSetValue()
        {
            String code =
@"
    a = 1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            mirror.SetValueAndExecute("a", 2);
            thisTest.Verify("a", 2);
        }

        [Test]
        public void MinimalSetValuePropagate()
        {
            String code =
@"
    a = 1;
    b = a + 1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 2);
            mirror.SetValueAndExecute("a", 2);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 3);
        }

        [Test]
        public void MinimalSetValuePropagateFunction()
        {
            String code =
@"
    def foo(a) { return = a * 2; }
    a = 1;
    b = a + 1;
    c = foo(b);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 4);
            mirror.SetValueAndExecute("a", 2);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 3);
            thisTest.Verify("c", 6);
        }

        [Test]
        [Category("Failure")]
        public void AssocSetValue()
        {
            String code =
@"
a;b;c;
    [Associative]
{
    def foo(a) { return = a * 2; }
    a = 1;
    b = a + 1;
    c = foo(b);
}
";
            string defectID = "MAGN-1537 Regression in SetValueAndExecute API";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1537
            ExecutionMirror mirror = thisTest.RunScriptSource(code, defectID);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 4);
            mirror.SetValueAndExecute("a", 2);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 3);
            thisTest.Verify("c", 6);
        }
    }
}
