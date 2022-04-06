using Dynamo.Configuration;
using NUnit.Framework;

namespace Dynamo.Tests.Configuration
{
    [TestFixture]
    class ContextTests : DynamoModelTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void TestIsUnix()
        {
            Assert.IsFalse(Context.IsUnix);   
        }
    }
}
