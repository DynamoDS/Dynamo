using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoTestFx.TD;

namespace ProtoTest.DebugTests
{
    class NamespaceConflictTest : ProtoTestBase
    {
        [Test]
        [Category("Trace")]
        [Category("Failure")]
        public void DupImportTest()
        {
            var mirror = thisTest.RunScriptSource(
@"import(""Builtin.dll"");import(""FFITarget.dll"");
a = A.DupTargetTest.DupTargetTest(); 
aO = a.Foo();

b = B.DupTargetTest.DupTargetTest();
bO = b.Foo();
"
);
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1947
            string defectID = "MAGN-1947 IntegrationTests.NamespaceConflictTest.DupImportTest";
            thisTest.Verify("aO", 1);
            thisTest.Verify("bO", 2);

        }

        [Test]
        public void DupImportTestNamespaceConflict01()
        {
            var mirror = thisTest.RunScriptSource(
@"import(""Builtin.dll"");import(""FFITarget.dll"");
a = DupTargetTest.DupTargetTest(); 
aO = a.Foo();
"
);
            thisTest.VerifyBuildWarningCount(ProtoCore.BuildData.WarningID.MultipleSymbolFoundFromName, 1);
        }

        [Test]
        public void DupImportTestNamespaceConflict02()
        {
            var mirror = thisTest.RunScriptSource(
@"import(""Builtin.dll"");import(""FFITarget.dll"");
a = DupTargetTest.DupTargetTest(); 
p = a;
"
);
            thisTest.VerifyBuildWarningCount(ProtoCore.BuildData.WarningID.MultipleSymbolFoundFromName, 1);
            thisTest.Verify("p", null);
        }


        [Test]
        [Category("Trace")]
        public void DupImportTestNeg()
        {
            var mirror = thisTest.RunScriptSource(
@"import(""Builtin.dll"");import(""FFITarget.dll"");
a = DupTargetTest.DupTargetTest(); 
aO = a.Foo();
"
);
        }
    }
}
