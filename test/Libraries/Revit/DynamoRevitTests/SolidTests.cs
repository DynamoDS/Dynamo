//using System.IO;
//using System.Linq;
//using Autodesk.Revit.DB;
//using Dynamo.Nodes;
//using Dynamo.Utilities;
//using NUnit.Framework;

//namespace Dynamo.Tests
//{
//    class SolidTests:DynamoRevitUnitTestBase
//    {
//        [Test]
//        public void BlendSolid()
//        {
//            var model = dynSettings.Controller.DynamoModel;

//            string samplePath = Path.Combine(_testPath, @".\Solid\BlendSolid.dyn");
//            string testPath = Path.GetFullPath(samplePath);

//            model.Open(testPath);
//            dynSettings.Controller.RunExpression(true);

//            var blendNode = dynSettings.Controller.DynamoModel.Nodes.First(x => x is CreateBlendGeometry);
//            var result = (Solid)VisualizationManager.GetDrawablesFromNode(blendNode).Values.First();
//            double volumeMin = 3700000.0;
//            double volumeMax = 3900000.0;
//            double actualVolume = result.Volume;
//            Assert.Greater(actualVolume, volumeMin);
//            Assert.Less(actualVolume, volumeMax);
//        }

//        [Test]
//        public void Loft()
//        {
//            var model = dynSettings.Controller.DynamoModel;

//            string samplePath = Path.Combine(_testPath, @".\Solid\Loft.dyn");
//            string testPath = Path.GetFullPath(samplePath);

//            model.Open(testPath);
//            dynSettings.Controller.RunExpression(true);

//            var fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
//            fec.OfClass(typeof(GenericForm));

//            //verify one loft created
//            int count = fec.ToElements().Count;
//            Assert.IsInstanceOf(typeof(Form), fec.ToElements().First());
//            Assert.AreEqual(1, count);
//        }

//        [Test]
//        public void RevolveSolid()
//        {
//            var model = dynSettings.Controller.DynamoModel;

//            string samplePath = Path.Combine(_testPath, @".\Solid\RevolveSolid.dyn");
//            string testPath = Path.GetFullPath(samplePath);

//            model.Open(testPath);
//            dynSettings.Controller.RunExpression(true);

//            var revolveNode = dynSettings.Controller.DynamoModel.Nodes.First(x => x is CreateRevolvedGeometry);
//            var result = (Solid)VisualizationManager.GetDrawablesFromNode(revolveNode).Values.First();
//            double volumeMin = 13300.0;
//            double volumeMax = 13550.0;
//            double actualVolume = result.Volume;
//            Assert.Greater(actualVolume, volumeMin);
//            Assert.Less(actualVolume, volumeMax);
//        }

//        [Test]
//        public void SolidBySkeleton()
//        {
//            if (!DocumentManager.GetInstance().CurrentUIApplication.Application.VersionNumber.Contains("2013") &&
//                             DocumentManager.GetInstance().CurrentUIApplication.Application.VersionName.Contains("Vasari"))
//            {
//                var model = dynSettings.Controller.DynamoModel;

//                string samplePath = Path.Combine(_testPath, @".\Solid\SolidBySkeleton.dyn");
//                string testPath = Path.GetFullPath(samplePath);

//                model.Open(testPath);
//                dynSettings.Controller.RunExpression(true);

//                var skeletonNode = dynSettings.Controller.DynamoModel.Nodes.First(x => x is SkinCurveLoops);
//                var result = (Solid)VisualizationManager.GetDrawablesFromNode(skeletonNode).Values.First();
//                double volumeMin = 82500.0;
//                double volumeMax = 84500.0;
//                double actualVolume = result.Volume;
//                Assert.Greater(actualVolume, volumeMin);
//                Assert.Less(actualVolume, volumeMax);
//            }
//        }

//        [Test]
//        public void SweepToMakeSolid()
//        {
//            var model = dynSettings.Controller.DynamoModel;

//            string samplePath = Path.Combine(_testPath, @".\Solid\SweepToMakeSolid.dyn");
//            string testPath = Path.GetFullPath(samplePath);

//            model.Open(testPath);
//            dynSettings.Controller.RunExpression(true);

//            var sweepNode = dynSettings.Controller.DynamoModel.Nodes.First(x => x is CreateSweptGeometry);
//            var result = (Solid)VisualizationManager.GetDrawablesFromNode(sweepNode).Values.First();
//            double volumeMin = 11800.0;
//            double volumeMax = 12150.0;
//            double actualVolume = result.Volume;
//            Assert.Greater(actualVolume, volumeMin);
//            Assert.Less(actualVolume, volumeMax);
//        }

//        [Test]
//        public void SweptBlend()
//        {
//            var model = dynSettings.Controller.DynamoModel;
//            System.Reflection.Assembly revitAPIAssembly = System.Reflection.Assembly.GetAssembly(typeof(GenericForm));
//            var FreeFormType = revitAPIAssembly.GetType("Autodesk.Revit.DB.FreeFormElement", false);
//            if (FreeFormType == null)
//                Assert.Inconclusive("FreeFormType not available.");

//            string samplePath = Path.Combine(_testPath, @".\Solid\SweptBlend.dyn");
//            string testPath = Path.GetFullPath(samplePath);

//            model.Open(testPath);
//            dynSettings.Controller.RunExpression(true);

//            var fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
//            fec.OfClass(typeof(GenericForm));

//            //verify one loft created
//            int count = fec.ToElements().Count;

//            Assert.IsInstanceOf(FreeFormType, fec.ToElements().First());
//            Assert.AreEqual(1, count);
//        }

//        [Test]
//        public void BoxByCenterAndDimensions()
//        {
//            var model = dynSettings.Controller.DynamoModel;

//            string samplePath = Path.Combine(_testPath, @".\Solid\BoxByCenterAndDimensions.dyn");
//            string testPath = Path.GetFullPath(samplePath);

//            model.Open(testPath);
//            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
//        }

//        [Test]
//        public void BoxByTwoCorners()
//        {
//            var model = dynSettings.Controller.DynamoModel;

//            string samplePath = Path.Combine(_testPath, @".\Solid\BoxByTwoCorners.dyn");
//            string testPath = Path.GetFullPath(samplePath);

//            model.Open(testPath);
//            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
//        }
//    }
//}
