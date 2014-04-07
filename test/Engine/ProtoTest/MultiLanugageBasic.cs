using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest
{
    [TestFixture]
    class MultiLanugageBasic
    {
        [SetUp]
        public void TestSetup()
        {
        }
        [Test]
        public void TestSingleLanguageImperative()
        {
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            fsr.Execute(
@"[Imperative]{    a = 3;    b = 4;}", core);
        }
        [Test]
        public void TestSingleLanguageAssociative()
        {
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            fsr.Execute(
@"[Associative]{    a = 3;    b = 4;}", core);
        }
        [Test]
        public void TestMultLanguageAssociativeImperative()
        {
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            fsr.Execute(
@"[Associative]{    a = [Imperative]        {            return= 5;        }    b = 4;}", core);
        }
        [Test]
        public void TestMultLanguageImperativeAssociative()
        {
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            fsr.Execute(
@"[Imperative]{    [Associative]    {        return= 5;    }    b = 4;}", core);
        }
        [Test]
        public void TestMultLanguageVariableUsage()
        {
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();

            fsr.Execute(
@"[Associative]{    a = 2;    [Imperative]    {        if(a == 2 )        {            b = a + 5;            a = 20;        }        else         {            b = 4;        }    }    c = a;}", core);
        }
        [Test]
        public void TestClassUsageInImpeartive()
        {
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(
@"class foo{	m : var;	constructor Create(a1 : int)	{		m = a1;	}}x;y;[Imperative]{	p = foo.Create(16);    x = p.m;    p.m = 32;    y = p.m;}"
    , core);
            Assert.IsTrue((Int64)mirror.GetValue("x", 0).Payload == 16);
            Assert.IsTrue((Int64)mirror.GetValue("y", 0).Payload == 32);
        }
    }
}
