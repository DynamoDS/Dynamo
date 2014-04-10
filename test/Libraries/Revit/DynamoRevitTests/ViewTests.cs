using System.IO;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class ViewTests:DynamoRevitUnitTestBase
    {

        [Test]
        [TestModel(@".\View\AxonometricView.rfa")]
        public void AxonometricView()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\View\AxonometricView.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        [TestModel(@".\View\OverrideElementColorInView.rvt")]
        public void OverrideElementColorInView()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\View\OverrideElementColorInView.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        [TestModel(@".\View\PerspectiveView.rfa")]
        public void PerspectiveView()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\View\PerspectiveView.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }
    }
}
