using System.IO;

using NUnit.Framework;

using RTF.Framework;

namespace RevitIntegrationTests
{
    [TestFixture]
    class DividedCurveTests : IntegrationTest
    {
        [Test]
        [TestModel(@".\DividedCurve\DividedCurve.rfa")]
        public void DividedCurve()
        {
            string samplePath = Path.Combine(workingDirectory, @".\DividedCurve\DividedCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }
    }
}
