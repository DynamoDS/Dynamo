using NUnit.Framework;

namespace ProtoTest.DebugTests
{
    class NamespaceConflictTest : ProtoTestBase
    {
        [Test]
        [Category("Trace")]
        public void DupImportTest()
        {
            var mirror = thisTest.RunScriptSource(
@"import(""FFITarget.dll"");
a = A.DupTargetTest.DupTargetTest(); 
aO = a.Foo();

b = B.DupTargetTest.DupTargetTest();
bO = b.Foo();
"
);
            thisTest.VerifyBuildWarningCount(ProtoCore.BuildData.WarningID.MultipleSymbolFoundFromName, 1);
            thisTest.Verify("aO", 1);
            thisTest.Verify("bO", null);

        }

        [Test]
        public void DupImportTestNamespaceConflict01()
        {
            var mirror = thisTest.RunScriptSource(
@"import(""FFITarget.dll"");
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
@"import(""FFITarget.dll"");
a = DupTargetTest.DupTargetTest(); 
p = a;
"
);
            thisTest.VerifyBuildWarningCount(ProtoCore.BuildData.WarningID.MultipleSymbolFoundFromName, 1);
            thisTest.Verify("p", null);
        }
    }
}
