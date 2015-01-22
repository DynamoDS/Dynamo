using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    class RedefinitionExpression : ProtoTestBase
    {
        [Test]
        public void RedefineWithFunctions01()
        {
            String code =
@"def f(i : int){    return = i + 1;}x = 1000;x = f(x);";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 1001);
        }

        [Test]
        [Category("ToFixYuKe")]
        public void RedefineWithFunctions02()
        {
            String code =
@"class C{    mx : var;    constructor C(i : int)    {        mx = i + 1;    }}p = 10;p = C.C(p);x = p.mx;";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 11);
        }

        [Test]
        public void RedefineWithFunctions03()
        {
            String code =
@"class C{    mx : var;    constructor C()    {        mx = 10;    }    def f(a : int)    {        mx = a + 1;        return = mx;    }}x = 10;p = C.C();x = p.f(x);";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 11);
        }
        //TestCase from Mark//

        [Test]
        public void RedefineWithFunctions04()
        {
            String code =
@"def f1(i : int, k : int){return = i + k;}def f2(i : int, k : int){return = i - k;}x = 12;y = 10;x = f1(x, y) - f2(x, y); ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 20);
        }

        [Test]
        public void RedefineWithFunctions05()
        {
            String code =
@"def f(i : int){i = i * i;return = i;}x = 2;x = f(x + f(x));";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 36);
        }

        [Test]
        public void RedefineWithExpressionLists01()
        {
            String code =
@"a = 1;a = {a, 2};x = a[0];y = a[1];";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 2);
        }

        [Test]
        public void RedefineWithExpressionLists02()
        {
            String code =
@"def f(i : int){    return = i + 1;}a = 1;a = {1, f(a)};x = a[0];y = a[1];";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 2);
        }
        //Mark TestCases//

        [Test]
        [Category("ToFixJun")]
        public void RedefineWithExpressionLists03()
        {
            String code =
@"def f(i : int){    return = i + list[i];}list = {1, 2, 3, 4};a = 1;a = {f(f(a)), f(a)};x = a[0];y = a[1];";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 7);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 3);
        }

        [Test]
        public void RedefineWithExpressionLists04()
        {
            String code =
@"class C{    x : var[];    constructor C()    {        x = {1, 2, 3, 4, 5, 6};    }    def f(a : int)    {        x = x[a] * x[a + 1];        return = x;    }}x = 2;p = C.C();x = p.f(x);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 12 });
        }
    }
}
