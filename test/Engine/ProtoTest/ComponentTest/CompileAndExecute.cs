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

        [Test]
        public void TestCompilerAndRuntimeComponent01()
        {

            String code =
@"a = 10;";
            // Compile core
            var opts = new Options();
            opts.ExecutionMode = ExecutionMode.Serial;
            ProtoCore.Core core = new Core(opts);
            core.Compilers.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.kImperative, new ProtoImperative.Compiler(core));
            ProtoScriptTestRunner runner = new ProtoScriptTestRunner();

            // Compiler instance
            ProtoCore.DSASM.Executable dsExecutable;
            bool compileSucceeded = runner.CompileMe(code, core, out dsExecutable);
            Assert.IsTrue(compileSucceeded == true);
            
            // Pass compile data to the runtime 
            RuntimeCore runtimeCore = new RuntimeCore(core.Heap);
            runtimeCore.SetProperties(core.Options, dsExecutable);

            // Runtime
            ExecutionMirror mirror = runner.ExecuteMe(runtimeCore);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 10);
        }

       

    }
}