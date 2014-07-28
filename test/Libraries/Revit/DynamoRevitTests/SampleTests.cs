using System;
using System.IO;
using Dynamo.Nodes;
using Dynamo.Selection;
using System.Linq;
using Dynamo.Utilities;
using NUnit.Framework;
using DSCoreNodesUI;
using DSCore.File;
using Revit.Elements;
using RTF.Framework;
using Autodesk.DesignScript.Geometry;

namespace Dynamo.Tests
{
    [TestFixture]
    class SampleTests : DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void CreatePointSequenceSample()
        {
            var model = dynSettings.Controller.DynamoModel;
            OpenModel(@".\01 Create Point\create point_sequence.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);
            AssertNoDummyNodes();

            // evaluate  graph
            RunCurrentModel();

            var refPtNodeId = "d615cc73-d32d-4b1f-b519-0b8f9b903ebf";
            AssertPreviewCount(refPtNodeId, 9);

            // get 8th reference point
            var refPt = GetPreviewValueAtIndex(refPtNodeId, 8) as ReferencePoint;
            Assert.IsNotNull(refPt);
            Assert.AreEqual(80, refPt.Z);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CreatePointEndSample()
        {
            var model = dynSettings.Controller.DynamoModel;
            OpenModel(@".\01 Create Point\create point - end.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);
            AssertNoDummyNodes();

            RunCurrentModel();

            // test copying and pasting the workflow
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.AddRange(dynSettings.Controller.DynamoModel.Nodes);
            model.Copy(null);
            model.Paste(null);

            // evaluate graph
            var refPtNodeId = "16d1ceb2-c780-45d1-9dfb-d9c49836a931";
            var refPt = GetPreviewValue(refPtNodeId) as ReferencePoint;
            Assert.IsNotNull(refPt);
            Assert.AreEqual(0, refPt.Z);

            // change slider value and re-evaluate graph
            DoubleSlider slider = model.CurrentWorkspace.NodeFromWorkspace("2eb70bdb-773d-4cf4-a10e-828dd39a0cca") as DoubleSlider;
            slider.Value = 56.78;
            RunCurrentModel();

            refPt = GetPreviewValue(refPtNodeId) as ReferencePoint;
            Assert.IsNotNull(refPt);
            Assert.AreEqual(56.78, refPt.Z);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CreatePointSample()
        {
            var model = dynSettings.Controller.DynamoModel;
            OpenModel(@".\01 Create Point\create point.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(2, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(1, model.CurrentWorkspace.Connectors.Count);
            AssertNoDummyNodes();

            // evaluate graph
            RunCurrentModel();

            var refPtNodeId = "f4088a7b-823a-49e8-936c-3c56d1a99455";
            var refPt = GetPreviewValue(refPtNodeId) as ReferencePoint;
            Assert.IsNotNull(refPt);
            Assert.AreEqual(0, refPt.Z);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void RefGridSlidersSample()
        {
            var model = dynSettings.Controller.DynamoModel;
            OpenModel(@".\02 Ref Grid Sliders\ref grid sliders.dyn");

            // check all the nodes and connectors are loaded
            Assert.GreaterOrEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.GreaterOrEqual(10, model.CurrentWorkspace.Connectors.Count);
            AssertNoDummyNodes();

            // evaluate graph
            RunCurrentModel();

            var refPtNodeId = "69dcdcdc-941f-46f9-8e8b-242b61e74e80";
            AssertPreviewCount(refPtNodeId, 36);

            var refPt = GetPreviewValueAtIndex(refPtNodeId, 3) as ReferencePoint;
            Assert.IsNotNull(refPt);
            Assert.AreEqual(57, refPt.Y);

            // change slider value and re-evaluate graph
            DoubleSlider slider = model.CurrentWorkspace.NodeFromWorkspace("5adff29b-3cac-4387-8d1d-b75ceb9c6dec") as DoubleSlider;
            slider.Value = 3.5;

            RunCurrentModel();
            AssertPreviewCount(refPtNodeId, 16);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void RefGridSlidersEndSample()
        {
            var model = dynSettings.Controller.DynamoModel;
            OpenModel(@".\02 Ref Grid Sliders\ref grid sliders - end.dyn");

            // check all the nodes and connectors are loaded
            Assert.GreaterOrEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.GreaterOrEqual(11, model.CurrentWorkspace.Connectors.Count);
            AssertNoDummyNodes();

            // evaluate graph
            RunCurrentModel();

            var famInstNodeId = "fc83b9b2-42c6-4a9f-8f60-a6ee29ef8a34";
            AssertPreviewCount(famInstNodeId, 36);

            var famInst = GetPreviewValueAtIndex(famInstNodeId, 3) as FamilyInstance;
            Assert.IsNotNull(famInst);
            Assert.IsNotNullOrEmpty(famInst.Name);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void DivideSelectedCurveEndSample()
        {
            //var model = dynSettings.Controller.DynamoModel;

            //ModelCurve mc1;
            //CreateOneModelCurve(out mc1);

            //string samplePath = Path.Combine(_samplesPath, @".\03 Divide Selected Curve\divide selected curve - end.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.Open(testPath);
            //var selectionNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is CurvesBySelection);
            //Assert.AreEqual(1, selectionNodes.Count());

            //((CurvesBySelection)selectionNodes.ElementAt(0)).SelectedElement = mc1;

            //Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            Assert.Inconclusive("Porting : CurveBySelection");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void DivideSelectedCurveSample()
        {
            //var model = dynSettings.Controller.DynamoModel;

            //ModelCurve mc1;
            //CreateOneModelCurve(out mc1);

            //string samplePath = Path.Combine(_samplesPath, @".\03 Divide Selected Curve\divide selected curve.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.Open(testPath);
            //var selectionNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is CurvesBySelection);
            //Assert.AreEqual(1, selectionNodes.Count());

            //((CurvesBySelection)selectionNodes.ElementAt(0)).SelectedElement = mc1;

            //Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            Assert.Inconclusive("Porting : CurveBySelection");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void FormFromCurveSelectionListSample()
        {
            //var model = dynSettings.Controller.DynamoModel;

            //ModelCurve mc1;
            //ModelCurve mc2;
            //CreateTwoModelCurves(out mc1, out mc2);

            //string samplePath = Path.Combine(_samplesPath, @".\04 Form From Curve Selection\form from curve selection.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.Open(testPath);

            ////get the two selection nodes in the sample
            //var selectionNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is CurvesBySelection);
            //Assert.AreEqual(2, selectionNodes.Count());

            //((CurvesBySelection)selectionNodes.ElementAt(0)).SelectedElement = mc1;
            //((CurvesBySelection)selectionNodes.ElementAt(1)).SelectedElement = mc2;

            //Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            Assert.Inconclusive("Porting : CurveBySelection");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void FormFromCurveSelectionSample()
        {
            //var model = dynSettings.Controller.DynamoModel;

            //ModelCurve mc1;
            //ModelCurve mc2;
            //CreateTwoModelCurves(out mc1, out mc2);

            //string samplePath = Path.Combine(_samplesPath, @".\04 Form From Curve Selection\form from curve selection.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.Open(testPath);

            ////populate the selection nodes in the sample
            //var selectionNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is CurvesBySelection);
            //((CurvesBySelection)selectionNodes.ElementAt(0)).SelectedElement = mc1;
            //((CurvesBySelection)selectionNodes.ElementAt(1)).SelectedElement = mc2;

            //Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            Assert.Inconclusive("Porting : CurveBySelection");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void GraphFunctionAndConnectPointsSample()
        {
            string samplePath = Path.Combine(_samplesPath, @".\05 Graph Function\graph function and connect points.dyn");
            string testPath = Path.GetFullPath(samplePath);

            string customDefPath1 = Path.Combine(_defsPath, "GraphFunction.dyf");
            string customDefPath2 = Path.Combine(_defsPath, "ConnectPoints.dyf");
            Assert.IsTrue(File.Exists(customDefPath1), "Cannot find specified custom definition to load for testing.");
            Assert.IsTrue(File.Exists(customDefPath2), "Cannot find specified custom definition to load for testing.");

            Assert.IsTrue(dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath1) != null);
            Assert.IsTrue(dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath2) != null);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ScalableGraphFunctionSample()
        {
            string samplePath = Path.Combine(_samplesPath, @".\05 Graph Function\scalable graph function.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void GraphFunctionSample()
        {
            string samplePath = Path.Combine(_samplesPath, @".\05 Graph Function\graph function.dyn");
            string testPath = Path.GetFullPath(samplePath);

            string customDefPath = Path.Combine(_defsPath, "GraphFunction.dyf");
            Assert.IsTrue(File.Exists(customDefPath), "Cannot find specified custom definition to load for testing.");
            Assert.IsTrue(dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath) != null);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            dynSettings.Controller.RunExpression();
        }

        [Test]
        [TestModel(@"..\..\..\doc\distrib\Samples\08 Get Set Family Params\inst param.rvt")]
        public void InstParamSample()
        {
            string samplePath = Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            dynSettings.Controller.RunExpression();
        }

        [Test]
        [TestModel(@"..\..\..\doc\distrib\Samples\08 Get Set Family Params\inst param mass families.rvt")]
        public void InstParam2MassesSample()
        {
            string samplePath = Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param 2 masses.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            dynSettings.Controller.RunExpression();
        }

        [Test]
        [TestModel(@"..\..\..\doc\distrib\Samples\08 Get Set Family Params\inst param mass families.rvt")]
        public void InstParam2MassesDrivingEachOtherSample()
        {
            string samplePath = Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param 2 masses.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        [Test]
        public void ParametricTowerSamples()
        {
            Assert.Inconclusive();
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Attractor_1()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\10 Attractor\Attractor Logic_End.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(20, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Attractor_2()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\10 Attractor\Attractor Logic_Start.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(17, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(17, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        [Test]
        [TestModel(@"..\..\..\doc\distrib\Samples\11 Indexed Family Instances\IndexedFamilyInstances.rfa")]
        public void IndexedFamilyInstances()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\11 Indexed Family Instances\Indexed Family Instances.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(13, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void AdaptiveComponentPlacement()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\18 Adaptive Components\Adaptive Component Placement.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        [Test]
        [TestModel(@"..\..\..\doc\distrib\Samples\16 Tesselation\tesselation.rfa")]
        public void Tesselation_1()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\16 Tesselation\2dDomain.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(17, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(23, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

            //var watch = model.CurrentWorkspace.NodeFromWorkspace<NewList>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");
            //FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            //Assert.AreEqual(0, actual.Length);

        }

        [Test]
        [TestModel(@"..\..\..\doc\distrib\Samples\16 Tesselation\tesselation.rfa")]
        public void Tesselation_2()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\16 Tesselation\tesselation with coincident grids.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(16, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Test]
        [TestModel(@"..\..\..\doc\distrib\Samples\16 Tesselation\tesselation.rfa")]
        public void Tesselation_3()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\16 Tesselation\tesselation.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Test]
        [TestModel(@"..\..\..\doc\distrib\Samples\16 Tesselation\tesselation.rfa")]
        public void Tesselation_4()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\16 Tesselation\tesselation_types.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Transforms_TranslateAndRotatesequence()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\17 Transforms\Translate and Rotate sequence.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(17, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(18, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Transforms_TranslateAndRotate()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\17 Transforms\Translate and Rotate.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(14, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(14, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Formulas_FormulaCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\19 Formulas\FormulaCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(17, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Formulas_ScalableCircle()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\19 Formulas\Scalable Circle.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Spreadsheets_ExcelToStuff()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\15 Spreadsheets\Excel to Stuff.dyn");
            string testPath = Path.GetFullPath(samplePath);
            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(22, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(18, model.CurrentWorkspace.Connectors.Count);

            var workspace = model.CurrentWorkspace;
            var filePickerNode = workspace.FirstNodeFromWorkspace<Filename>();

            // remap the file name as Excel requires an absolute path
            var excelFilePath = Path.Combine(_samplesPath, @".\15 Spreadsheets\");
            //excelFilePath = Path.Combine(excelFilePath, excelFileName);
            excelFilePath = Path.Combine(excelFilePath, "helix.xlsx");
            filePickerNode.Value = excelFilePath;

            Assert.IsFalse(string.IsNullOrEmpty(excelFilePath));
            Assert.IsTrue(File.Exists(excelFilePath));

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

            //Assert.Inconclusive("Porting : StringFileName");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Spreadsheets_CSVToStuff()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\15 Spreadsheets\CSV to Stuff.dyn");
            string testPath = Path.GetFullPath(samplePath);
            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);

            var workspace = model.CurrentWorkspace;
            var filePickerNode = workspace.FirstNodeFromWorkspace<Filename>();

            // remap the file name as CSV requires an absolute path
            var excelFilePath = Path.Combine(_samplesPath, @".\15 Spreadsheets\");
            excelFilePath = Path.Combine(excelFilePath, "helix_smaller.csv");

            filePickerNode.Value = excelFilePath;

            Assert.IsFalse(string.IsNullOrEmpty(excelFilePath));
            Assert.IsTrue(File.Exists(excelFilePath));

            //dynSettings.Controller.RunExpression(true);
            //Assert.Inconclusive("Porting : StringFileName");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Rendering_hill_climbing_simple()
        {
            // referencing the samples directly from the samples folder
            // and the custom nodes from the distrib folder

            var model = dynSettings.Controller.DynamoModel;
            // look at the sample folder and one directory up to get the distrib folder and combine with defs folder
            string customNodePath = Path.Combine(Path.Combine(_samplesPath, @"..\\"), @".\
\Dynamo Sample Custom Nodes\dyf\");
            // get the full path to the distrib folder and def folder
            string fullCustomNodePath = Path.GetFullPath(customNodePath);

            string samplePath = Path.Combine(_samplesPath, @".\25 Rendering\hill_climbing_simple.dyn");
            string testPath = Path.GetFullPath(samplePath);

            // make sure that the two custom nodes we need exist
            string customDefPath1 = Path.Combine(fullCustomNodePath, "ProduceChild.dyf");
            string customDefPath2 = Path.Combine(fullCustomNodePath, "DecideNewParent.dyf");

            Assert.IsTrue(File.Exists(customDefPath1), "Cannot find specified custom definition to load for testing at." + customDefPath1);
            Assert.IsTrue(File.Exists(customDefPath2), "Cannot find specified custom definition to load for testing." + customDefPath2);

            Assert.DoesNotThrow(() =>
                         dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath2));
            Assert.DoesNotThrow(() =>
                          dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath1));



            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            Assert.AreEqual(2, dynSettings.Controller.CustomNodeManager.LoadedCustomNodes.Count);
            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());


            var workspace = model.CurrentWorkspace;

            Assert.Fail("Mike to update for CB2B1");

            //var produceChildCustomNode =
            //    (Function)workspace.Nodes.First(x => x is Function);
            ////// ensure that recursive custom nodes returns a list
            //Assert.IsTrue(produceChildCustomNode.OldValue.IsList);
            //var resultList =((FScheme.Value.List)produceChildCustomNode.OldValue).Item;

            ////// ensure that last item is a 0, we return a 0 for the last item in the recursive call to make sure the recursion has returned something
            //var lastItemInList = resultList[resultList.Length - 1].GetDoubleFromFSchemeValue();
            // Assert.AreEqual(0, lastItemInList);

            ////// the second to last item in the list should be our solution to the hill climbing problem - it should be within 10 ft of point 100,100,100
            //var secondToLastItem = resultList[resultList.Length - 2];
            ////// get xyz from this fscheme object
            //var xyzSecondToLastItem = secondToLastItem.GetObjectFromFSchemeValue<XYZ>();
            //var distance = xyzSecondToLastItem.DistanceTo(new XYZ(100,100,100));
            //Assert.LessOrEqual(distance, 10);


        }

        #region 14 Curves

        [Test]
        [TestModel(@".\Samples\AllCurves.rfa")]
        public void AllCurveTestModelCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\all curve test model curve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(47, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(61, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Test]
        [TestModel(@".\Samples\AllCurves.rfa")]
        public void AllCurveTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\all curve test.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(33, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(33, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Test]
        [TestModel(@".\Samples\AllCurves.rfa")]
        public void ArcAndLineFromRefPoints()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\Arc and Line from Ref Points.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(15, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Test]
        [TestModel(@".\Samples\AllCurves.rfa")]
        public void ArcAndLine()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\Arc and Line.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(17, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(16, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Test]
        [TestModel(@".\Samples\AllCurves.rfa")]
        public void ArcFromRefPoints()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\Arc from Ref Points.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Test]
        [TestModel(@".\Samples\AllCurves.rfa")]
        public void Arc()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\Arc.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(13, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Test]
        [TestModel(@".\Samples\AllCurves.rfa")]
        public void Circle()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\circle.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(13, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Test]
        [TestModel(@".\Samples\AllCurves.rfa")]
        public void Ellipse()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\ellipse.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(14, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(14, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        #endregion

        #region 06 Python Node

        [Test]
        [TestModel(@".\Samples\AllCurves.rfa")]
        public void ConnectTwoPointArraysWithoutPython()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\connect two point arrays without python.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var nodes = Controller.DynamoModel.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            // check all the nodes and connectors are loaded
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Ignore]
        public void ConnectTwoPointArrays()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\connect two point arrays.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Ignore]
        public void CreateSineWaveFromSelectedCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\create sine wave from selected curve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(2, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }

        [Test]
        public void CreateSineWaveFromSelectedPoints()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\create sine wave from selected points.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

        }



        #endregion

        #region New Samples Tests

        [Test, Category("Samples")]
        [TestModel(@".\Samples\DynamoSample.rvt")]
        public void Revit_Adaptive_Component_Placement()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Samples\Revit_Adaptive Component Placement.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();


            var refPtNodeId = "357e7a53-361c-4c1e-81ae-83e16213a39a";
            AssertPreviewCount(refPtNodeId, 9);

            // get all AdaptiveComponent.
            for (int i = 0; i <= 8; i++)
            {
                var refPt = GetPreviewValueAtIndex(refPtNodeId, i) as AdaptiveComponent;
                Assert.IsNotNull(refPt);
            }

            // change slider value and re-evaluate graph
            IntegerSlider slider = model.CurrentWorkspace.NodeFromWorkspace
                ("cc3ba87a-cc1f-4db6-99f2-769f3020e0df") as IntegerSlider;
            slider.Value = 6;

            RunCurrentModel();

            AssertPreviewCount(refPtNodeId, 6);

            // Now there should be only 6 AdaptiveComponent in Revit.
            for (int i = 0; i <= 5; i++)
            {
                var refPt = GetPreviewValueAtIndex(refPtNodeId, i) as AdaptiveComponent;
                Assert.IsNotNull(refPt);
            }

        }

        [Test, Category("Samples")]
        [TestModel(@".\Samples\DynamoSample.rvt")]
        public void Revit_Color()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Samples\Revit_Color.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(16, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(21, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            var refPtNodeId = "ecb2936d-6ee9-4b99-9ab1-24269ffedfc5";
            AssertPreviewCount(refPtNodeId, 10);

            // get all Walls.
            for (int i = 0; i <= 9; i++)
            {
                var refPt = GetPreviewValueAtIndex(refPtNodeId, i) as Wall;
                Assert.IsNotNull(refPt);
            }
        }

        [Ignore, Category("Samples")]
        [TestModel(@".\Samples\DynamoSample.rvt")]
        public void Revit_Floors_and_Framing()
        {
            // this test marked as Ignore because on running it is throwing error from Revit side.
            // if I run it manually there is no error. Will discuss this with Ian

            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Samples\Revit_Floors and Framing.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(31, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(35, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            var floorByTypeAndLevel = "4074e4e4-c6ee-4413-8cbb-cc9af5b6127f";
            AssertPreviewCount(floorByTypeAndLevel, 5);

            // get all Floors.
            for (int i = 0; i <= 4; i++)
            {
                var floors = GetPreviewValueAtIndex(floorByTypeAndLevel, i) as Floor;
                Assert.IsNotNull(floors);
            }

            var structuralFraming = "205a479f-3b9e-4f4f-866e-6901fde3d9ca";
            AssertPreviewCount(structuralFraming, 20);

            // get all Structural Framing elements
            for (int i = 0; i <= 19; i++)
            {
                var framing = GetPreviewValueAtIndex(structuralFraming, i) as StructuralFraming;
                Assert.IsNotNull(framing);
            }
        }

        [Test, Category("Samples")]
        [TestModel(@".\Samples\DynamoSample.rvt")]
        public void Revit_ImportSolid()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Samples\Revit_ImportSolid.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(18, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(20, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            var NodeId = "e3fedc00-247a-4971-901c-7fcb063344c6";

            // get imported geometry instance.
            var geometryInstance = GetPreviewValue(NodeId) as ImportInstance;
            Assert.IsNotNull(geometryInstance);

        }

        [Test, Category("Samples")]
        [TestModel(@".\Samples\DynamoSample.rvt")]
        public void Revit_PlaceFamiliesByLevel_Set_Parameters()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine
                (_testPath, @".\Samples\Revit_PlaceFamiliesByLevel_Set Parameters.dyn");

            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(14, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(17, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            var familyInstance = "026aadc9-644e-4e6c-b35c-bf1aec67045c";
            AssertPreviewCount(familyInstance, 27);

            // get all Family instances.
            for (int i = 0; i <= 26; i++)
            {
                var family = GetPreviewValueAtIndex(familyInstance, i) as FamilyInstance;
                Assert.IsNotNull(family);
            }
        }

        [Test, Category("Samples")]
        [TestModel(@".\Samples\DynamoSample.rvt")]
        public void Revit_StructuralFraming()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine
                (_testPath, @".\Samples\Revit_StructuralFraming.dyn");

            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(15, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            var familyInstance = "e98dab6a-e6bc-4da8-84b6-d756caee48fe";
            AssertPreviewCount(familyInstance, 9);

            // get all Families.
            for (int i = 0; i <= 8; i++)
            {
                var family = GetPreviewValueAtIndex(familyInstance, i) as StructuralFraming;
                Assert.IsNotNull(family);
            }
        }

        [Test, Category("Samples")]
        [TestModel(@".\empty.rfa")]
        public void Geometry_Curves()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Samples\Geometry_Curves.dyn");

            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(20, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(22, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            // Validation for Model Curves.
            var modelCurve = "e8fc9b8f-03ce-4351-8333-6ae47a62ac07";
            AssertPreviewCount(modelCurve, 4);

            for (int i = 0; i <= 3; i++)
            {
                var curve = GetPreviewValueAtIndex(modelCurve, i) as ModelCurve;
                Assert.IsNotNull(curve);
            }
        }

        [Test, Category("Samples")]
        [TestModel(@".\empty.rfa")]
        public void Geometry_Points()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Samples\Geometry_Points.dyn");

            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            // Validation for Reference Points.
            var refPoints = "6e4226cf-674b-4c94-9c78-7818af53464e";
            AssertPreviewCount(refPoints, 11);

            for (int i = 0; i <= 10; i++)
            {
                var points = GetPreviewValueAtIndex(refPoints, i) as ReferencePoint;
                Assert.IsNotNull(points);
            }
        }

        [Test, Category("Samples")]
        [TestModel(@".\empty.rfa")]
        public void Geometry_Solids()
        {
            var model = dynSettings.Controller.DynamoModel;
            OpenModel(@".\Geometry\Geometry_Solids.dyn");

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(43, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(51, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            // Validation of Form creation
            var formByLoft = "d8b7c8f4-2f70-4e3b-a913-91316e009759";
            var surface = GetPreviewValue(formByLoft) as Form;
            Assert.IsNotNull(surface);

            // Validation for Geometry Instance import.
            var geometryInstance = "e0e441ca-08a4-48b7-8f64-96e65e320376";
            var solid = GetPreviewValue(geometryInstance) as ImportInstance;
            Assert.IsNotNull(solid);

            // Validation for Solids.
            var trimmedSolid = "66746ec0-c2bd-4a5c-a365-63fe4ca239ba";
            var solid1 = GetPreviewValueAtIndex(trimmedSolid, 0) as Solid;
            Assert.IsNotNull(solid1);

        }

        [Test, Category("Samples")]
        [TestModel(@".\empty.rfa")]
        public void Geometry_Surfaces()
        {
            var model = dynSettings.Controller.DynamoModel;
            OpenModel(@".\Geometry\Geometry_Surfaces.dyn");

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(42, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(49, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            // Validation of Form creation
            var formByLoft = "ae4bfdfe-524a-4a5c-bd64-75fd56be89af";
            var surface = GetPreviewValue(formByLoft) as Form;
            Assert.IsNotNull(surface);

            // Validation for Geometry Instance import.
            var geometryInstance = "c1e10fd1-974e-4374-b6ec-0766615d3df4";
            var solid = GetPreviewValue(geometryInstance) as ImportInstance;
            Assert.IsNotNull(solid);

            // Validation for NurbsSurface
            var nurbsSurface = "4859e415-f137-4590-b107-c528167769c0";
            var surface1 = GetPreviewValue(nurbsSurface) as NurbsSurface;
            Assert.IsNotNull(surface1);

            // Validation for Surface
            var extrudedSurface = "660a5c8e-c828-45ff-849e-2d65e04c7206";
            var surface2 = GetPreviewValue(extrudedSurface) as Surface;
            Assert.IsNotNull(surface2);

        }
        #endregion
    }

}
