using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace ProtoTest.UtilsTests
{
    using NUnit.Framework;
    using ProtoCore.DSASM.Mirror;
    using ProtoScript.Runners;
    [TestFixture]
    class CoreDumpTest : ProtoTestBase
    {
        private ProtoScriptTestRunner coreRunner = null;
        public override void Setup()
        {
            base.Setup();
            coreRunner = new ProtoScript.Runners.ProtoScriptTestRunner();
        }
        [Test]
        public void TestArrayCoreDump01()
        {
            string sourceCode = @"                class A                {                    c : int = 0;                        constructor B(v : int)                    {                        c = v;                    }                }                values = A.B(1..20..1);";
            ExecutionMirror mirror = coreRunner.Execute(sourceCode, core);
            List<string> globalVariables = null;
            mirror.GetCoreDump(out globalVariables, 7, 4);
            Assert.AreEqual(1, globalVariables.Count);
            Assert.AreEqual("values = { A(c = 1), A(c = 2), A(c = 3), ..., A(c = 18), A(c = 19), A(c = 20) }", globalVariables[0]);
        }
        [Test]
        public void TestArrayCoreDump02() // Test array truncation size being an even number.
        {
            string sourceCode = @"                class A                {                    c : int = 0;                        constructor B(v : int)                    {                        c = v;                    }                }                under = A.B(1..7..1);                match = A.B(1..8..1);                over = A.B(1..9..1);";
            ExecutionMirror mirror = coreRunner.Execute(sourceCode, core);
            List<string> globalVariables = null;
            mirror.GetCoreDump(out globalVariables, 8, 4);
            Assert.AreEqual(3, globalVariables.Count);
            Assert.AreEqual("under = { A(c = 1), A(c = 2), A(c = 3), A(c = 4), A(c = 5), A(c = 6), A(c = 7) }", globalVariables[0]);
            Assert.AreEqual("match = { A(c = 1), A(c = 2), A(c = 3), A(c = 4), A(c = 5), A(c = 6), A(c = 7), A(c = 8) }", globalVariables[1]);
            Assert.AreEqual("over = { A(c = 1), A(c = 2), A(c = 3), A(c = 4), ..., A(c = 6), A(c = 7), A(c = 8), A(c = 9) }", globalVariables[2]);
        }
        [Test]
        public void TestArrayCoreDump03() // Test array truncation size being an odd number.
        {
            string sourceCode = @"                class A                {                    c : int = 0;                        constructor B(v : int)                    {                        c = v;                    }                }                under = A.B(1..6..1);                match = A.B(1..7..1);                over = A.B(1..8..1);";
            ExecutionMirror mirror = coreRunner.Execute(sourceCode, core);
            List<string> globalVariables = null;
            mirror.GetCoreDump(out globalVariables, 7, 4);
            Assert.AreEqual(3, globalVariables.Count);
            Assert.AreEqual("under = { A(c = 1), A(c = 2), A(c = 3), A(c = 4), A(c = 5), A(c = 6) }", globalVariables[0]);
            Assert.AreEqual("match = { A(c = 1), A(c = 2), A(c = 3), A(c = 4), A(c = 5), A(c = 6), A(c = 7) }", globalVariables[1]);
            Assert.AreEqual("over = { A(c = 1), A(c = 2), A(c = 3), ..., A(c = 6), A(c = 7), A(c = 8) }", globalVariables[2]);
        }
        [Test]
        public void TestRecursiveCoreDump()
        {
            string sourceCode = @"                class A                {                    x;                }                a = A.A();                x = a.x;                a.x = a;";
            ExecutionMirror mirror = coreRunner.Execute(sourceCode, core);
            List<string> globalVariables = null;
            mirror.GetCoreDump(out globalVariables, 7, 4);
            // Fix: http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-398
            Assert.AreEqual(2, globalVariables.Count);
            Assert.AreEqual("a = A(x = A(x = A(x = ...)))", globalVariables[0]);
            Assert.AreEqual("x = A(x = A(x = A(x = ...)))", globalVariables[1]);
        }
    }
}
