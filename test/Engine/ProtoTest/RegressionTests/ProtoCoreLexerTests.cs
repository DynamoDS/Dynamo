using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
namespace ProtoTest.RegressionTests
{
    [TestFixture]
    public class LexerRegressionTests
    {
        public ProtoCore.Core core;
        [SetUp]
        public void Setup()
        {
            core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
        }

        [Test]
        public void PreClarifyPreParseBracket001()
        {
            String code =
@"x;[Associative]{ a = {1001,1002};     // This is failing the pre-parse.  // Probably has somthing to do with convertingthe language blocks into binary exprs     x = a[0];  }";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Obj o = mirror.GetValue("x");
            Assert.IsTrue((Int64)o.Payload == 1001);
        }
    }
}
