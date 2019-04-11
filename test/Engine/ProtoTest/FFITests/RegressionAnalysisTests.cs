using NUnit.Framework;
using ProtoCore.DSASM.Mirror;

namespace ProtoTest.FFITests
{
    [TestFixture]
    class RegressionAnalysisTests : ProtoTestBase
    {
        [Test]
        public void TestArrayPromoteAndCastIList()
        {
            string code = @"
import(""FFITarget.dll"");
a = 5;
o1 = RegressionTargets.AverageIList(a);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("o1", 5.0);

        }

        [Test]
        public void TestArrayPromoteAndCastList()
        {
            string code = @"
import(""FFITarget.dll"");
a = 5;
o1 = RegressionTargets.AverageList(a);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("o1", 5.0);

        }


    }
}
