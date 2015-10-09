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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ExecutionMirror mirror = fsr.Execute(code, core, out runtimeCore);
            Obj o = mirror.GetValue("size");
            Assert.IsTrue((Int64)o.Payload == 5);
        }
    }
}
