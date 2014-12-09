using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoFFI;
using ProtoScript.Runners;
using ProtoTest.TD;
using ProtoTestFx.TD;
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
            StackValue svA = mirror.GetValue("a").DsasmValue;
            Assert.IsTrue(svA.IsInteger);
            Assert.IsTrue(svA.opdata == 1);
            mirror.SetValueAndExecute("a", 2);
            StackValue svA2 = mirror.GetValue("a").DsasmValue;
            Assert.IsTrue(svA2.IsInteger);
            Assert.IsTrue(svA2.opdata == 2);
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
            StackValue svA = mirror.GetValue("a").DsasmValue;
            Assert.IsTrue(svA.IsInteger);
            Assert.IsTrue(svA.opdata == 1);
            StackValue svB = mirror.GetValue("b").DsasmValue;
            Assert.IsTrue(svB.IsInteger);
            Assert.IsTrue(svB.opdata == 2);
            mirror.SetValueAndExecute("a", 2);
            StackValue svA2 = mirror.GetValue("a").DsasmValue;
            Assert.IsTrue(svA2.IsInteger);
            Assert.IsTrue(svA2.opdata == 2);
            StackValue svB2 = mirror.GetValue("b").DsasmValue;
            Assert.IsTrue(svB2.IsInteger);
            Assert.IsTrue(svB2.opdata == 3);
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
            StackValue svA = mirror.GetValue("a").DsasmValue;
            Assert.IsTrue(svA.IsInteger);
            Assert.IsTrue(svA.opdata == 1);
            StackValue svB = mirror.GetValue("b").DsasmValue;
            Assert.IsTrue(svB.IsInteger);
            Assert.IsTrue(svB.opdata == 2);
            StackValue svC = mirror.GetValue("c").DsasmValue;
            Assert.IsTrue(svC.IsInteger);
            Assert.IsTrue(svC.opdata == 4);
            mirror.SetValueAndExecute("a", 2);
            StackValue svA2 = mirror.GetValue("a").DsasmValue;
            Assert.IsTrue(svA2.IsInteger);
            Assert.IsTrue(svA2.opdata == 2);
            StackValue svB2 = mirror.GetValue("b").DsasmValue;
            Assert.IsTrue(svB2.IsInteger);
            Assert.IsTrue(svB2.opdata == 3);
            StackValue svC2 = mirror.GetValue("c").DsasmValue;
            Assert.IsTrue(svC2.IsInteger);
            Assert.IsTrue(svC2.opdata == 6);
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
            StackValue svA = mirror.GetValue("a").DsasmValue;
            Assert.IsTrue(svA.IsInteger);
            Assert.IsTrue(svA.opdata == 1);
            StackValue svB = mirror.GetValue("b").DsasmValue;
            Assert.IsTrue(svB.IsInteger);
            Assert.IsTrue(svB.opdata == 2);
            StackValue svC = mirror.GetValue("c").DsasmValue;
            Assert.IsTrue(svC.IsInteger);
            Assert.IsTrue(svC.opdata == 4);

            mirror.SetValueAndExecute("a", 2);
            StackValue svA2 = mirror.GetValue("a").DsasmValue;
            Assert.IsTrue(svA2.IsInteger);
            Assert.IsTrue(svA2.opdata == 2, defectID);
            StackValue svB2 = mirror.GetValue("b").DsasmValue;
            Assert.IsTrue(svB2.IsInteger);
            Assert.IsTrue(svB2.opdata == 3, defectID);
            StackValue svC2 = mirror.GetValue("c").DsasmValue;
            Assert.IsTrue(svC2.IsInteger);
            Assert.IsTrue(svC2.opdata == 6, defectID);
        }
    }
}
