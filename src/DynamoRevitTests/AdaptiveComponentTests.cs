using System.IO;
using System.Linq;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class AdaptiveComponentTests:DynamoRevitUnitTestBase
    {
        [Test]
        public void AdaptiveComponentByFace()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\AdaptiveComponent\AdaptiveComponentByFace.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void AdaptiveComponentByCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\AdaptiveComponent\AdaptiveComponentByCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void AdaptiveComponent()
        {
            var model = dynSettings.Controller.DynamoModel;

            //string path = Path.Combine(_testPath, @".\AdaptiveComponent.rfa");
            //string modelPath = Path.GetFullPath(path);
            //SwapCurrentModel(modelPath);

            string samplePath = Path.Combine(_testPath, @".\AdaptiveComponent\AdaptiveComponent.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            //the .dyn has the slider set at 5. let's make sure that
            //if you set the slider to something else before running, that it get the correct number
            var slider = dynSettings.Controller.DynamoModel.Nodes.First(x => x is DoubleSliderInput);
            ((BasicInteractive<double>)slider).Value = 1;

            dynSettings.Controller.RunExpression(true);

            //get all the family instances in the document
            var acs = TestUtils.GetAllFamilyInstancesWithTypeName("3PointAC_wireTruss");
            Assert.AreEqual(1, acs.Count());

            //change the number slider
            ((BasicInteractive<double>)slider).Value = 3;

            dynSettings.Controller.RunExpression(true);
            acs = TestUtils.GetAllFamilyInstancesWithTypeName("3PointAC_wireTruss");
            Assert.AreEqual(3, acs.Count());

        }
    }
}
