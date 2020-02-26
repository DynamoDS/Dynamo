using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.MultiLangTests
{
    class NonExistingFunctionTest : ProtoTestBase
    {
        // This test exercises the code change made in https://github.com/DynamoDS/Dynamo/pull/10024
        // Note that this test only verifies that the code being run does not crash the VM
        [Test]
        public void DYN_2093_NullInput_NonExisting_flatten_withoutAssignment()
        {
            var code =
@"
import(""DSCoreNodes.dll"");
DSCore.List.flatten(null);
";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void DYN_2093_NullInput_NonExisting_flatten_withAssignment()
        {
            var code =
@"
import(""DSCoreNodes.dll"");
a = DSCore.List.flatten(null);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
        }

        // This test is to verify that calling a non-existent static (flatten) function 
        // with a non-null list would still not crash.
        [Test]
        public void DYN_2093_NonNullInput_NonExisting_flatten()
        {
            var code =
@"
import(""DSCoreNodes.dll"");
a = [[1..2], [3..4]];
b = DSCore.List.flatten(a);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", null);
        }

        // This test is to verify that the actual Flatten call is functioning as expected.
        [Test]
        public void DYN_2093_NonNullInput_Existing_Flatten()
        {
            var code =
@"
import(""DSCoreNodes.dll"");
a = [[1..2], [3..4]];
b = DSCore.List.Flatten(a);
l0 = b[0];
l1 = b[1];
l2 = b[2];
l3 = b[3];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("l0", 1);
            thisTest.Verify("l1", 2);
            thisTest.Verify("l2", 3);
            thisTest.Verify("l3", 4);
        }
    }
}
