using Dynamo.Core;
using NUnit.Framework;

namespace Dynamo.Tests.Core
{
    [TestFixture]
    public class CrashPromptArgsTests : DynamoModelTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void CrashPromptArgsTest()
        {
            //All the parameters filled
            CrashPromptArgs cpa = new CrashPromptArgs("Details", "Override Text", "File Path");

            Assert.IsTrue(cpa.HasDetails());
            Assert.IsTrue(cpa.IsDefaultTextOverridden());
            Assert.IsTrue(cpa.IsFilePath());

            //No parameters filled
            CrashPromptArgs cpa3 = new CrashPromptArgs(null);

            Assert.IsFalse(cpa3.HasDetails());
            Assert.IsFalse(cpa3.IsDefaultTextOverridden());
            Assert.IsFalse(cpa3.IsFilePath());
        }
    }
}