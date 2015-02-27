using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
namespace ProtoTest.Associative
{
    class ExecutionMirrorTests : ProtoTestBase
    {
        [Test]
        public void LiteralRetrival()
        {
            String code =
@"foo;[Associative]{	foo = 5;}";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Obj o = mirror.GetValue("foo");
            Assert.IsTrue((Int64)o.Payload == 5);
        }

        [Test]
        public void ArrayRetrival1D()
        {
            String code =
@"foo;[Associative]{	foo = {5};}";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Obj o = mirror.GetValue("foo");
            ProtoCore.DSASM.Mirror.DsasmArray a = (ProtoCore.DSASM.Mirror.DsasmArray)o.Payload;
            Assert.IsTrue(a.members.Length == 1);
            Assert.IsTrue((Int64)a.members[0].Payload == 5);
        }

        [Test]
        public void ArrayRetrival2D()
        {
            String code =
@"foo;[Associative]{	foo = {{5}};}";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Obj o = mirror.GetValue("foo");
            ProtoCore.DSASM.Mirror.DsasmArray a = (ProtoCore.DSASM.Mirror.DsasmArray)o.Payload;
            Assert.IsTrue(a.members.Length == 1);
            ProtoCore.DSASM.Mirror.DsasmArray a2 = (ProtoCore.DSASM.Mirror.DsasmArray)a.members[0].Payload;
            Assert.IsTrue(a2.members.Length == 1);

            Assert.IsTrue((Int64)a2.members[0].Payload == 5);
        }

        [Test]
        public void ArrayRetrival2DJagged()
        {
            String code =
@"foo;[Associative]{	foo = {{5}, 6};}";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Obj o = mirror.GetValue("foo");
            ProtoCore.DSASM.Mirror.DsasmArray a = (ProtoCore.DSASM.Mirror.DsasmArray)o.Payload;
            Assert.IsTrue(a.members.Length == 2);
            ProtoCore.DSASM.Mirror.DsasmArray a2 = (ProtoCore.DSASM.Mirror.DsasmArray)((a.members[0]).Payload);
            Assert.IsTrue(a2.members.Length == 1);
            Assert.IsTrue((Int64)a2.members[0].Payload == 5);
            Assert.IsTrue((Int64)a.members[1].Payload == 6);
        }

        [Test]
        public void ArrayRetrival2D2b1()
        {
            String code =
@"foo;[Associative]{	foo = {{5}, {6}};}";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Obj o = mirror.GetValue("foo");
            ProtoCore.DSASM.Mirror.DsasmArray a = (ProtoCore.DSASM.Mirror.DsasmArray)o.Payload;
            Assert.IsTrue(a.members.Length == 2);
            ProtoCore.DSASM.Mirror.DsasmArray a2 = (ProtoCore.DSASM.Mirror.DsasmArray)((a.members[0]).Payload);
            Assert.IsTrue(a2.members.Length == 1);
            Assert.IsTrue((Int64)a2.members[0].Payload == 5);
            ProtoCore.DSASM.Mirror.DsasmArray a3 = (ProtoCore.DSASM.Mirror.DsasmArray)((a.members[1]).Payload);
            Assert.IsTrue(a3.members.Length == 1);
            Assert.IsTrue((Int64)a3.members[0].Payload == 6);
        }

        [Test]
        public void ArrayRetrival1DEmpty()
        {
            String code =
@"foo;[Associative]{	foo = {};}";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            ProtoCore.Lang.Obj o = mirror.GetValue("foo");
            ProtoCore.DSASM.Mirror.DsasmArray a = (ProtoCore.DSASM.Mirror.DsasmArray)o.Payload;
            Assert.IsTrue(a.members.Length == 0);
        }
    }
}
