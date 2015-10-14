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
    class CompileAndExecute : ProtoTestBase
    {
        [Test]
        public void TestCompilerAndRuntimeComponent01()
        {

            String code =
@"// DesignScript code herea = 10;";
            // Compile
            ProtoScriptRunner runner = new ProtoScriptRunner();
            bool compileSucceeded = runner.CompileAndGenerateExe(code, core, new ProtoCore.CompileTime.Context());
            Assert.IsTrue(compileSucceeded == true);
            
            // Execute
            runtimeCore = runner.ExecuteVM(core);

            // Verify
            ExecutionMirror mirror = new ExecutionMirror(runtimeCore.CurrentExecutive.CurrentDSASMExec, runtimeCore);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 10);
        }
    }
}