using Dynamo.Tests;
using NUnit.Framework;

namespace RevitTestFrameworkRunner
{
    [TestFixture]
    class Diagnostics
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void WillShutdownBeforeProcessFinishes()
        {
            System.Threading.Thread.Sleep(300000);
            Assert.Fail("If you've made it here, the timeout was not honored.");
        }

        [Test]
        [TestModel(@".\AModelThatDoesNotExist.rfa")]
        public void WillShutdownIfJournalCompletionFails()
        {
            Assert.Fail("If you've made it here, the timeout was not honored.");
        }
    }
}
