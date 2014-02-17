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
    class SetValueTests
    {
        public ProtoCore.Core core;
        private DebugRunner fsr;
        private ProtoScript.Config.RunConfiguration runnerConfig;
        private string testPath = @"..\..\..\test\Engine\ProtoTest\ImportFiles\";
        public TestFrameWork thisTest = new TestFrameWork();
        [SetUp]
        public void Setup()
        {
            // Specify some of the requirements of IDE.
            var options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            options.SuppressBuildOutput = false;
            core = new ProtoCore.Core(options);
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            fsr = new DebugRunner(core);
            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();
        }

        [Test]
        public void MinimalSetValue()
        {
            String code =
@"    a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            StackValue svA = mirror.GetValue("a").DsasmValue;
            Assert.IsTrue(svA.optype == AddressType.Int);
            Assert.IsTrue(svA.opdata == 1);
            mirror.SetValueAndExecute("a", 2);
            StackValue svA2 = mirror.GetValue("a").DsasmValue;
            Assert.IsTrue(svA2.optype == AddressType.Int);
            Assert.IsTrue(svA2.opdata == 2);
        }

        [Test]
        public void MinimalSetValuePropagate()
        {
            String code =
@"    a = 1;    b = a + 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            StackValue svA = mirror.GetValue("a").DsasmValue;
            Assert.IsTrue(svA.optype == AddressType.Int);
            Assert.IsTrue(svA.opdata == 1);
            StackValue svB = mirror.GetValue("b").DsasmValue;
            Assert.IsTrue(svB.optype == AddressType.Int);
            Assert.IsTrue(svB.opdata == 2);
            mirror.SetValueAndExecute("a", 2);
            StackValue svA2 = mirror.GetValue("a").DsasmValue;
            Assert.IsTrue(svA2.optype == AddressType.Int);
            Assert.IsTrue(svA2.opdata == 2);
            StackValue svB2 = mirror.GetValue("b").DsasmValue;
            Assert.IsTrue(svB2.optype == AddressType.Int);
            Assert.IsTrue(svB2.opdata == 3);
        }

        [Test]
        public void MinimalSetValuePropagateFunction()
        {
            String code =
@"    def foo(a) { return = a * 2; }    a = 1;    b = a + 1;    c = foo(b);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            StackValue svA = mirror.GetValue("a").DsasmValue;
            Assert.IsTrue(svA.optype == AddressType.Int);
            Assert.IsTrue(svA.opdata == 1);
            StackValue svB = mirror.GetValue("b").DsasmValue;
            Assert.IsTrue(svB.optype == AddressType.Int);
            Assert.IsTrue(svB.opdata == 2);
            StackValue svC = mirror.GetValue("c").DsasmValue;
            Assert.IsTrue(svC.optype == AddressType.Int);
            Assert.IsTrue(svC.opdata == 4);
            mirror.SetValueAndExecute("a", 2);
            StackValue svA2 = mirror.GetValue("a").DsasmValue;
            Assert.IsTrue(svA2.optype == AddressType.Int);
            Assert.IsTrue(svA2.opdata == 2);
            StackValue svB2 = mirror.GetValue("b").DsasmValue;
            Assert.IsTrue(svB2.optype == AddressType.Int);
            Assert.IsTrue(svB2.opdata == 3);
            StackValue svC2 = mirror.GetValue("c").DsasmValue;
            Assert.IsTrue(svC2.optype == AddressType.Int);
            Assert.IsTrue(svC2.opdata == 6);
        }

        [Test]
        public void AssocSetValue()
        {
            String code =
@"a;b;c;    [Associative]{    def foo(a) { return = a * 2; }    a = 1;    b = a + 1;    c = foo(b);}";
            // Defect : DNL-1467620 Regression in SetValueAndExecute API
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            StackValue svA = mirror.GetValue("a").DsasmValue;
            Assert.IsTrue(svA.optype == AddressType.Int);
            Assert.IsTrue(svA.opdata == 1);
            StackValue svB = mirror.GetValue("b").DsasmValue;
            Assert.IsTrue(svB.optype == AddressType.Int);
            Assert.IsTrue(svB.opdata == 2);
            StackValue svC = mirror.GetValue("c").DsasmValue;
            Assert.IsTrue(svC.optype == AddressType.Int);
            Assert.IsTrue(svC.opdata == 4);
            mirror.SetValueAndExecute("a", 2);
            StackValue svA2 = mirror.GetValue("a").DsasmValue;
            Assert.IsTrue(svA2.optype == AddressType.Int);
            Assert.IsTrue(svA2.opdata == 2);
            StackValue svB2 = mirror.GetValue("b").DsasmValue;
            Assert.IsTrue(svB2.optype == AddressType.Int);
            Assert.IsTrue(svB2.opdata == 3);
            StackValue svC2 = mirror.GetValue("c").DsasmValue;
            Assert.IsTrue(svC2.optype == AddressType.Int);
            Assert.IsTrue(svC2.opdata == 6);
        }
    }
}
