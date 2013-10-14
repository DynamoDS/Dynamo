using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class SolidTests:DynamoRevitUnitTestBase
    {
        [Test]
        public void BlendSolid()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\BlendSolid.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            var blendNode = dynSettings.Controller.DynamoModel.Nodes.First(x => x is CreateBlendGeometry);
            var result = (Solid)VisualizationManager.GetDrawablesFromNode(blendNode).First();
            double volumeMin = 3700000.0;
            double volumeMax = 3900000.0;
            double actualVolume = result.Volume;
            Assert.Greater(actualVolume, volumeMin);
            Assert.Less(actualVolume, volumeMax);
        }

        [Test]
        public void Loft()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Loft.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            var fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(GenericForm));

            //verify one loft created
            int count = fec.ToElements().Count;
            Assert.IsInstanceOf(typeof(Form), fec.ToElements().First());
            Assert.AreEqual(1, count);
        }

        [Test]
        public void RevolveSolid()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\RevolveSolid.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            var revolveNode = dynSettings.Controller.DynamoModel.Nodes.First(x => x is CreateRevolvedGeometry);
            var result = (Solid)VisualizationManager.GetDrawablesFromNode(revolveNode).First();
            double volumeMin = 13300.0;
            double volumeMax = 13550.0;
            double actualVolume = result.Volume;
            Assert.Greater(actualVolume, volumeMin);
            Assert.Less(actualVolume, volumeMax);
        }

        [Test]
        public void SolidBySkeleton()
        {
            if (!dynRevitSettings.Revit.Application.VersionNumber.Contains("2013") &&
                             dynRevitSettings.Revit.Application.VersionName.Contains("Vasari"))
            {
                var model = dynSettings.Controller.DynamoModel;

                string samplePath = Path.Combine(_testPath, @".\SolidBySkeleton.dyn");
                string testPath = Path.GetFullPath(samplePath);

                model.Open(testPath);
                dynSettings.Controller.RunExpression(true);

                var skeletonNode = dynSettings.Controller.DynamoModel.Nodes.First(x => x is SkinCurveLoops);
                var result = (Solid)VisualizationManager.GetDrawablesFromNode(skeletonNode).First();
                double volumeMin = 82500.0;
                double volumeMax = 84500.0;
                double actualVolume = result.Volume;
                Assert.Greater(actualVolume, volumeMin);
                Assert.Less(actualVolume, volumeMax);
            }
        }

        [Test]
        public void SweepToMakeSolid()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\SweepToMakeSolid.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            var sweepNode = dynSettings.Controller.DynamoModel.Nodes.First(x => x is CreateSweptGeometry);
            var result = (Solid)VisualizationManager.GetDrawablesFromNode(sweepNode).First();
            double volumeMin = 11800.0;
            double volumeMax = 12150.0;
            double actualVolume = result.Volume;
            Assert.Greater(actualVolume, volumeMin);
            Assert.Less(actualVolume, volumeMax);
        }
    }
}
