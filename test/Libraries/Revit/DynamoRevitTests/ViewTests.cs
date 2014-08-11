using System.IO;
using Dynamo.Utilities;
using NUnit.Framework;
using RTF.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class ViewTests:DynamoRevitUnitTestBase
    {

        [Test]
        [TestModel(@".\View\AxonometricView.rfa")]
        public void AxonometricView()
        {
            string samplePath = Path.Combine(_testPath, @".\View\AxonometricView.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        [Test]
        [TestModel(@".\View\OverrideElementColorInView.rvt")]
        public void OverrideElementColorInView()
        {
            string samplePath = Path.Combine(_testPath, @".\View\OverrideElementColorInView.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        [Test]
        [TestModel(@".\View\PerspectiveView.rfa")]
        public void PerspectiveView()
        {
            string samplePath = Path.Combine(_testPath, @".\View\PerspectiveView.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }
    }
}
