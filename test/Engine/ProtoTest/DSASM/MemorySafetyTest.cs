using NUnit.Framework;

namespace ProtoTest.DSASM
{
    [TestFixture]
    class MemorySafetyTest : ProtoTestBase
    {
        [Test]
        public void TestMemoryAllocation()
        {
            // Arrays can be much bigger in net6 before an out of memory exception is thrown.
            // The magic number 2147483646 is the theoretical max size we can ask for for an array
            // and should guarantee an out of memory exception.
            string code = @"
x = [];
x[2147483646] = 21;
";
            thisTest.RunAndVerifyRuntimeWarning(code, ProtoCore.Runtime.WarningID.RunOutOfMemory);
        }
    }
}
