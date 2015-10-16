using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoTestFx.TD;

using ProtoCore;
using ProtoScript.Runners;

namespace ProtoTest.ComponentTest
{
    public class CompileAndExecute : ProtoTestBase
    {
        readonly string testCasePath = Path.GetFullPath(@"..\..\..\Scripts\Associative\MicroFeatureTests\");

        [Test, Category("Failure")]
        public void TestCompilerAndRuntimeComponent01()
        {

            String code =
@"// Any DS code goes herea = 10;";
            // Compile core
            var opts = new Options();
            opts.ExecutionMode = ExecutionMode.Serial;
            ProtoCore.Core core = new Core(opts);
            core.Compilers.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.kImperative, new ProtoImperative.Compiler(core));
            ProtoScriptRunner runner = new ProtoScriptRunner();

            // Compile
            bool compileSucceeded = runner.CompileAndGenerateExe(code, core);
            Assert.IsTrue(compileSucceeded == true);

            // Execute
            RuntimeCore runtimeCore = runner.ExecuteVM(core);

            // Verify
            ExecutionMirror mirror = new ExecutionMirror(runtimeCore.CurrentExecutive.CurrentDSASMExec, runtimeCore);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 10);
        }
    }
}