using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    public class SimpleVerification : ProtoTestBase
    {
        [Test]
        public void TestAssignment01()
        {

            String code =
@"a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("foo");
            Assert.IsTrue((Int64)o.Payload == 5);
        }

        [Test]
        public void TestAssignment02()
        {
            string code =
@"a = 2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 6);
        }

        [Test]
        public void TestArrayAssignment01()
        {
            string code =
@"a = 2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 6);
        }


        [Test]
        public void TestSyntaxError01()
        {
            String code =
            @"
// No semicolon
a = 10 
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                Object n1 = null;
                thisTest.RunScriptSource(code);
                thisTest.Verify("b", n1);
            });
        }

        [Test]
        [Category("Failure")]
        public void TestCompileWarning01()
        {
            const string code = @"
a = f();
";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(1);
            thisTest.Verify("temp", null);
        }

        [Test]
        public void TestRuntimeWarning01()
        {
            string code =

@"
a = 2 % 0;
b = 2.1 % 0;
";

            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("b", double.NaN);

            var warnings = thisTest.GetTestRuntimeCore().RuntimeStatus.Warnings;
            Assert.IsTrue(warnings.Any(w => w.ID == ProtoCore.Runtime.WarningID.kModuloByZero));
        }

        [Test]
        public void TestMultipleCompileWarnings01()
        {
            string code =
@"
        a : UndefinedType;
        b : UndefinedType2 = 2;
        c : UndefinedType3 = null;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);

            thisTest.VerifyRuntimeWarningCount(3);
        }

        [Test]
        public void TestStaticCyclicDependency01()
        {
            string code = @"
a = 1;
b = a;
a = b;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kInvalidStaticCyclicDependency);
            Object n1 = null;
            thisTest.Verify("a", n1);
            thisTest.Verify("b", n1);
        }

        [Test]
        public void TestRuntimeCyclicDependency01()
        {
            String errmsg = "1460274 - Sprint 18 : rev 1590 : Update issue : Cyclic dependency cases are going into infinite loop";
            string src = @"a;
b;
[Associative]
{
	a = 2;
        b = a *3;
        a = 6.5;
        a = b / 3; 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.VerifyRunScriptSource(src, errmsg);
            thisTest.Verify("b", null);
            thisTest.Verify("a", null);
        }
    }
}