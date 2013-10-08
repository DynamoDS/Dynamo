using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    public class TestExamples
    {
        [Test]
        public void TestOne()
        {
            Assert.Fail("This failed.");
        }

        [Test]
        public void TestTwo()
        {
            Assert.Inconclusive("This is inconclusive.");
        }

        [Test]
        public void TestThree()
        {
            //this will pass.
            Assert.AreEqual(0,0);
        } 
    }
    
}
