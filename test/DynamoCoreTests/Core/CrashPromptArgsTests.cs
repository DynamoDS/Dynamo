using Dynamo.Core;
using NUnit.Framework;

namespace Dynamo.Tests.Core
{
    [TestFixture]
    public class CrashPromptArgsTests : DynamoModelTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void CrashPromptArgsConstructorTest()
        {
            //All the parameters filled
            CrashPromptArgs cpa = new CrashPromptArgs("Details", "Override Text", "File Path");

            //Validate properties get correctly filled
            Assert.AreEqual("Details", cpa.Details);
            Assert.AreEqual("Override Text", cpa.OverridingText);
            Assert.AreEqual("File Path", cpa.FilePath);

            Assert.IsTrue(cpa.HasDetails());
            Assert.IsTrue(cpa.IsDefaultTextOverridden());
            Assert.IsTrue(cpa.IsFilePath());

            //No parameters filled
            cpa = new CrashPromptArgs(null);

            //Validate properties are null
            Assert.IsNull(cpa.Details);
            Assert.IsNull(cpa.OverridingText);
            Assert.IsNull(cpa.FilePath);

            Assert.IsFalse(cpa.HasDetails());
            Assert.IsFalse(cpa.IsDefaultTextOverridden());
            Assert.IsFalse(cpa.IsFilePath());
        }
    }
}