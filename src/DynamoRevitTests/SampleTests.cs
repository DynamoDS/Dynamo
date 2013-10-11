using System.IO;
using System.Linq;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using NUnit.Framework;
using ModelCurve = Autodesk.Revit.DB.ModelCurve;

namespace Dynamo.Tests
{
    [TestFixture]
    class SampleTests:DynamoRevitUnitTestBase
    {
        [Test]
        public void CreatePointSequenceSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\01 Create Point\create point_sequence.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);
        }

        [Test]
        public void CreatePointEndSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\01 Create Point\create point - end.dyn");
            string testPath = Path.GetFullPath(samplePath);

            //test running the expression
            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            //test copying and pasting the workflow
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.AddRange(dynSettings.Controller.DynamoModel.Nodes);
            model.Copy(null);
            model.Paste(null);
        }

        [Test]
        public void CreatePointSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\01 Create Point\create point.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);
        }

        [Test]
        public void RefGridSlidersSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\02 Ref Grid Sliders\ref grid sliders.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);
        }

        [Test]
        public void RefGridSlidersEndSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\02 Ref Grid Sliders\ref grid sliders - end.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);
        }

        [Test]
        public void DivideSelectedCurveEndSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            ModelCurve mc1;
            CreateOneModelCurve(out mc1);

            string samplePath = Path.Combine(_samplesPath, @".\03 Divide Selected Curve\divide selected curve - end.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            var selectionNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is CurvesBySelection);
            Assert.AreEqual(1, selectionNodes.Count());

            ((CurvesBySelection)selectionNodes.ElementAt(0)).SelectedElement = mc1;

            dynSettings.Controller.RunExpression(true);
        }

        [Test]
        public void DivideSelectedCurveSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            ModelCurve mc1;
            CreateOneModelCurve(out mc1);

            string samplePath = Path.Combine(_samplesPath, @".\03 Divide Selected Curve\divide selected curve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            var selectionNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is CurvesBySelection);
            Assert.AreEqual(1, selectionNodes.Count());

            ((CurvesBySelection)selectionNodes.ElementAt(0)).SelectedElement = mc1;

            dynSettings.Controller.RunExpression(true);
        }

        [Test]
        public void FormFromCurveSelectionListSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            ModelCurve mc1;
            ModelCurve mc2;
            CreateTwoModelCurves(out mc1, out mc2);

            string samplePath = Path.Combine(_samplesPath, @".\04 Form From Curve Selection\form from curve selection - list.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            //get the two selection nodes in the sample
            var selectionNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is CurvesBySelection);
            Assert.AreEqual(2, selectionNodes.Count());

            ((CurvesBySelection)selectionNodes.ElementAt(0)).SelectedElement = mc1;
            ((CurvesBySelection)selectionNodes.ElementAt(1)).SelectedElement = mc2;

            dynSettings.Controller.RunExpression(true);
        }

        [Test]
        public void FormFromCurveSelectionSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            ModelCurve mc1;
            ModelCurve mc2;
            CreateTwoModelCurves(out mc1, out mc2);

            string samplePath = Path.Combine(_samplesPath, @".\04 Form From Curve Selection\form from curve selection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            //populate the selection nodes in the sample
            var selectionNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is CurvesBySelection);
            ((CurvesBySelection)selectionNodes.ElementAt(0)).SelectedElement = mc1;
            ((CurvesBySelection)selectionNodes.ElementAt(1)).SelectedElement = mc2;

            dynSettings.Controller.RunExpression(true);
        }

        [Test]
        public void GraphFunctionAndConnectPointsSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\05 Graph Function\graph function and connect points.dyn");
            string testPath = Path.GetFullPath(samplePath);

            string customDefPath1 = Path.Combine(_defsPath, "GraphFunction.dyf");
            string customDefPath2 = Path.Combine(_defsPath, "ConnectPoints.dyf");
            Assert.IsTrue(File.Exists(customDefPath1), "Cannot find specified custom definition to load for testing.");
            Assert.IsTrue(File.Exists(customDefPath2), "Cannot find specified custom definition to load for testing.");

            Assert.IsTrue(dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath1) != null);
            Assert.IsTrue(dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath2) != null);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);
        }

        [Test]
        public void ScalableGraphFunctionSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\05 Graph Function\scalable graph function.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

        }

        [Test]
        public void GraphFunctionSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\05 Graph Function\graph function.dyn");
            string testPath = Path.GetFullPath(samplePath);

            string customDefPath = Path.Combine(_defsPath, "GraphFunction.dyf");
            Assert.IsTrue(File.Exists(customDefPath), "Cannot find specified custom definition to load for testing.");
            Assert.IsTrue(dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath) != null);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);
        }

        [Test]
        public void InstParamSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string modelPath = Path.GetFullPath(Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param.rvt"));
            SwapCurrentModel(modelPath);

            string samplePath = Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);
        }

        [Test]
        public void InstParam2MassesSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string modelPath = Path.GetFullPath(Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param mass families.rvt"));
            SwapCurrentModel(modelPath);

            string samplePath = Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param 2 masses.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);
        }

        [Test]
        public void InstParam2MassesDrivingEachOtherSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string modelPath = Path.GetFullPath(Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param mass families.rvt"));
            SwapCurrentModel(modelPath);

            string samplePath = Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param 2 masses driving each other.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);
        }

        [Test]
        public void ParametricTowerSamples()
        {
            Assert.Inconclusive();
        }
    }
}
