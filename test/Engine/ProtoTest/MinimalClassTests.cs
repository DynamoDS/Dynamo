using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
namespace ProtoTest
{
    class MinimalClassTests : ProtoTestBase
    {
        [Test]
        public void TestDS()
        {
            String code =
@"size;[Imperative]{	size = 5;}";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Obj o = mirror.GetValue("size");
            Assert.IsTrue((Int64)o.Payload == 5);
        }
    }
}
