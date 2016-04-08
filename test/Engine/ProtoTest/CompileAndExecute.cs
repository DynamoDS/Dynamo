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
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 10);
        }
    }
}