using System.IO;

using NUnit.Framework;

using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    class ModelTextTests : SystemTest
    {
        [Test]
        [TestModel(@".\ModelText\ModelText.rfa")]
        public void ModelText()
        {
            string samplePath = Path.Combine(workingDirectory, @".\ModelText\ModelText.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }
    }
}
