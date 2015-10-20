using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
namespace ProtoTest.RegressionTests
{
    [TestFixture]
    class LexerRegressionTests : ProtoTestBase
    {
        [Test]
        public void PreClarifyPreParseBracket001()
        {
            String code =
@"x;
[Associative]
{
 a = {1001,1002};    
 // This is failing the pre-parse. 
 // Probably has somthing to do with convertingthe language blocks into binary exprs
     x = a[0];  
}";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore = fsr.Execute(code, core); ExecutionMirror mirror = runtimeCore.Mirror;
            Obj o = mirror.GetValue("x");
            Assert.IsTrue((Int64)o.Payload == 1001);
        }
    }
}
