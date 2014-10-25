using System.IO;

using NUnit.Framework;

using RTF.Framework;

namespace RevitIntegrationTests
{
    [TestFixture]
    internal class UVTests : IntegrationTest
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void UVRandom()
        {
            string samplePath = Path.Combine(workingDirectory, @".\UV\UVRandom.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }
    }
}
