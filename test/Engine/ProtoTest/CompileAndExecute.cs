using System;
using NUnit.Framework;

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