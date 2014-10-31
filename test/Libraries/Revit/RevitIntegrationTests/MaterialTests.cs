using System.IO;

using NUnit.Framework;

using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    class MaterialTests : SystemTest
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
