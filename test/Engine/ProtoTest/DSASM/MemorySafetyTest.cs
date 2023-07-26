using NUnit.Framework;

namespace ProtoTest.DSASM
{
    [TestFixture]
    class MemorySafetyTest : ProtoTestBase
    {
        [Test]
        public void TestMemoryAllocation()
        {
            string code = @"
x = [];
x[2147483646] = 21;
";
            thisTest.RunAndVerifyRuntimeWarning(code, ProtoCore.Runtime.WarningID.RunOutOfMemory);
        }
    }
}
