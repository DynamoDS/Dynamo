using System.IO;

using NUnit.Framework;

using RTF.Framework;

namespace RevitIntegrationTests
{
    [TestFixture]
    class LengthTests : IntegrationTest
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void Length()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Length\Length.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }
    }
}
