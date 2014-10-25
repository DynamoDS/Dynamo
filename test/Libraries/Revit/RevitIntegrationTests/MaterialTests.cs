using System.IO;

using NUnit.Framework;

using RTF.Framework;

namespace RevitIntegrationTests
{
    [TestFixture]
    class MaterialTests : IntegrationTest
    {
        [Test]
        [TestModel(@".\Material\GetMaterialByName.rfa")]
        public void GetMaterialByName()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Material\GetMaterialByName.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }
    }
}
