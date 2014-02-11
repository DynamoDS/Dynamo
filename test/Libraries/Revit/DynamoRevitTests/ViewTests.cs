using System.IO;
using Dynamo.Core;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class ViewTests:DynamoRevitUnitTestBase
    {

        [Test]
        public void AxonometricView()
        {
            var model = DynamoSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\View\AxonometricView.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => DynamoSettings.Controller.RunExpression(true));
        }

        [Test]
        public void OverrideElementColorInView()
        {
            var model = DynamoSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\View\OverrideElementColorInView.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => DynamoSettings.Controller.RunExpression(true));
        }

        [Test]
        public void PerspectiveView()
        {
            var model = DynamoSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\View\PerspectiveView.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => DynamoSettings.Controller.RunExpression(true));
        }
    }
}
