﻿using System.IO;
using System.Linq;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using NUnit.Framework;
using ModelCurve = Autodesk.Revit.DB.ModelCurve;
using XYZ = Autodesk.Revit.DB.XYZ;
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
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void RefGridSlidersSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\02 Ref Grid Sliders\ref grid sliders.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void RefGridSlidersEndSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\02 Ref Grid Sliders\ref grid sliders - end.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
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

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
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

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void FormFromCurveSelectionListSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            ModelCurve mc1;
            ModelCurve mc2;
            CreateTwoModelCurves(out mc1, out mc2);

            string samplePath = Path.Combine(_samplesPath, @".\04 Form From Curve Selection\form from curve selection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            //get the two selection nodes in the sample
            var selectionNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is CurvesBySelection);
            Assert.AreEqual(2, selectionNodes.Count());

            ((CurvesBySelection)selectionNodes.ElementAt(0)).SelectedElement = mc1;
            ((CurvesBySelection)selectionNodes.ElementAt(1)).SelectedElement = mc2;

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
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

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
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
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void ScalableGraphFunctionSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\05 Graph Function\scalable graph function.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

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
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void InstParamSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void InstParam2MassesSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param 2 masses.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void InstParam2MassesDrivingEachOtherSample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param 2 masses.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void ParametricTowerSamples()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void Attractor_1()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\10 Attractor\Attractor Logic_End.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(20, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void Attractor_2()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\10 Attractor\Attractor Logic_Start.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(17, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(17, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void IndexedFamilyInstances()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\11 Indexed Family Instances\Indexed Family Instances.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(13, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Ignore]
        public void AdaptiveComponentPlacement()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\18 Adaptive Components\Adaptive Component Placement.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void Tesselation_1()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\16 Tesselation\2dDomain.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(17, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(23, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            //var watch = model.CurrentWorkspace.NodeFromWorkspace<NewList>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");
            //FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            //Assert.AreEqual(0, actual.Length);

        }

        [Test]
        public void Tesselation_2()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\16 Tesselation\tesselation with coincident grids.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(16, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        [Test]
        public void Tesselation_3()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\16 Tesselation\tesselation.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        [Ignore]
        public void Tesselation_4()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\16 Tesselation\tesselation_types.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        [Test]
        public void Transforms_TranslateAndRotatesequence()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\17 Transforms\Translate and Rotate sequence.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(14, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        [Test]
        public void Transforms_TranslateAndRotate()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\17 Transforms\Translate and Rotate.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        [Test]
        public void Formulas_FormulaCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\19 Formulas\FormulaCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(17, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        [Ignore]
        public void Formulas_ScalableCircle()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\19 Formulas\Scalable Circle.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        [Ignore]
        public void Spreadsheets_ExcelToStuff()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\15 Spreadsheets\Excel to Stuff.dyn");
            string testPath = Path.GetFullPath(samplePath);
            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(22, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(18, model.CurrentWorkspace.Connectors.Count);

            var workspace = model.CurrentWorkspace;
            var filePickerNode = workspace.FirstNodeFromWorkspace<StringFilename>();

            // remap the file name as Excel requires an absolute path
            var excelFilePath = Path.Combine(_samplesPath, @".\15 Spreadsheets\");
            //excelFilePath = Path.Combine(excelFilePath, excelFileName);
            excelFilePath = Path.Combine(excelFilePath, "helix.xlsx");
            filePickerNode.Value = excelFilePath;

            Assert.IsFalse(string.IsNullOrEmpty(excelFilePath));
            Assert.IsTrue(File.Exists(excelFilePath));

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void Spreadsheets_CSVToStuff()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_samplesPath, @".\15 Spreadsheets\CSV to Stuff.dyn");
            string testPath = Path.GetFullPath(samplePath);
            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            var workspace = model.CurrentWorkspace;
            var filePickerNode = workspace.FirstNodeFromWorkspace<StringFilename>();

            // remap the file name as CSV requires an absolute path
            var excelFilePath = Path.Combine(_samplesPath, @".\15 Spreadsheets\");
            excelFilePath = Path.Combine(excelFilePath, "helix_smaller.csv");

            filePickerNode.Value = excelFilePath;

            Assert.IsFalse(string.IsNullOrEmpty(excelFilePath));
            Assert.IsTrue(File.Exists(excelFilePath));

            //dynSettings.Controller.RunExpression(true);
        }
        [Test]
        public void Rendering_hill_climbing_simple()
        {
            // referencing the samples directly from the samples folder
            // and the custom nodes from the distrib folder

            var model = dynSettings.Controller.DynamoModel;
            // look at the sample folder and one directory up to get the distrib folder and combine with defs folder
            string customNodePath = Path.Combine(Path.Combine(_samplesPath,@"..\\"), @".\dynamo_packages\Dynamo Sample Custom Nodes\dyf\");
            // get the full path to the distrib folder and def folder
            string fullCustomNodePath = Path.GetFullPath(customNodePath);

            string samplePath = Path.Combine(_samplesPath, @".\25 Rendering\hill_climbing_simple.dyn");
            string testPath = Path.GetFullPath(samplePath);
           
            // make sure that the two custom nodes we need exist
            string customDefPath1 = Path.Combine(fullCustomNodePath, "ProduceChild.dyf");
            string customDefPath2 = Path.Combine(fullCustomNodePath, "DecideNewParent.dyf");

            Assert.IsTrue(File.Exists(customDefPath1), "Cannot find specified custom definition to load for testing at." + customDefPath1);
            Assert.IsTrue(File.Exists(customDefPath2), "Cannot find specified custom definition to load for testing."+ customDefPath2);
            
 Assert.DoesNotThrow(() =>
              dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath2));
 Assert.DoesNotThrow(() =>
               dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath1));



            model.Open(testPath);
            Assert.AreEqual(2, dynSettings.Controller.CustomNodeManager.LoadedCustomNodes.Count);
            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);
           
             Assert.DoesNotThrow(() =>dynSettings.Controller.RunExpression(true));

           
            var workspace = model.CurrentWorkspace;
            
            var produceChildCustomNode =
                (Function)workspace.Nodes.First(x => x is Function);
            //// ensure that recursive custom nodes returns a list
            Assert.IsTrue(produceChildCustomNode.OldValue.IsList);
            var resultList =((FScheme.Value.List)produceChildCustomNode.OldValue).Item;
           
            //// ensure that last item is a 0, we return a 0 for the last item in the recursive call to make sure the recursion has returned something
            var lastItemInList = resultList[resultList.Length - 1].GetDoubleFromFSchemeValue();
             Assert.AreEqual(0, lastItemInList);

            //// the second to last item in the list should be our solution to the hill climbing problem - it should be within 10 ft of point 100,100,100
            var secondToLastItem = resultList[resultList.Length - 2];
            //// get xyz from this fscheme object
            var xyzSecondToLastItem = secondToLastItem.GetObjectFromFSchemeValue<XYZ>();
            var distance = xyzSecondToLastItem.DistanceTo(new XYZ(100,100,100));
            Assert.LessOrEqual(distance, 10);
  

        }

        [Test]
        public void Rendering_Brute_Forcing_Parameters_loadsCustomNodes()
        {
            // referencing the samples directly from the samples folder
            // and the custom nodes from the distrib folder

            var model = dynSettings.Controller.DynamoModel;
            // look at the sample folder and one directory up to get the distrib folder and combine with defs folder
            string customNodePath = Path.Combine(Path.Combine(_samplesPath, @"..\\"), @".\dynamo_packages\Dynamo Sample Custom Nodes\dyf\");
            // get the full path to the distrib folder and def folder
            string fullCustomNodePath = Path.GetFullPath(customNodePath);

            string samplePath = Path.Combine(_samplesPath, @".\25 Rendering\Brute Forcing Parameters and Rendering.dyn");
            string testPath = Path.GetFullPath(samplePath);

            // make sure that the custom nodes we need exist, check if order is important...
            string customDefPath1 = Path.Combine(fullCustomNodePath, "Create Cloud Daylight Job and Send.dyf");
            string customDefPath2 = Path.Combine(fullCustomNodePath, "Create Skymodel and Environment.dyf");
            string customDefPath3 = Path.Combine(fullCustomNodePath, "GetDataFromRender_WriteImage.dyf");
            string customDefPath4 = Path.Combine(fullCustomNodePath, "Parse Weather Data.dyf");
            string customDefPath5 = Path.Combine(fullCustomNodePath, "RenderWithWeatherData_Return List of data.dyf");
            string customDefPath6 = Path.Combine(fullCustomNodePath, "UpdateParameter(s).dyf");
            string customDefPath7 = Path.Combine(fullCustomNodePath, "displayDataCall.dyf");
            string customDefPath8 = Path.Combine(fullCustomNodePath, "Display Daylighting Data On Surface.dyf");

            Assert.IsTrue(File.Exists(customDefPath1), "Cannot find specified custom definition to load for testing at." + customDefPath1);
            Assert.IsTrue(File.Exists(customDefPath2), "Cannot find specified custom definition to load for testing." + customDefPath2);
            Assert.IsTrue(File.Exists(customDefPath3), "Cannot find specified custom definition to load for testing." + customDefPath3);
            Assert.IsTrue(File.Exists(customDefPath4), "Cannot find specified custom definition to load for testing." + customDefPath4);
            Assert.IsTrue(File.Exists(customDefPath5), "Cannot find specified custom definition to load for testing." + customDefPath5);
            Assert.IsTrue(File.Exists(customDefPath6), "Cannot find specified custom definition to load for testing." + customDefPath6);
            Assert.IsTrue(File.Exists(customDefPath7), "Cannot find specified custom definition to load for testing." + customDefPath7);
            Assert.IsTrue(File.Exists(customDefPath8), "Cannot find specified custom definition to load for testing." + customDefPath8);

            Assert.DoesNotThrow(() =>
                         dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath2));
            Assert.DoesNotThrow(() =>
                          dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath1));
            Assert.DoesNotThrow(() =>
                           dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath3));
            Assert.DoesNotThrow(() =>
                           dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath4));
            Assert.DoesNotThrow(() =>
                           dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath5));
            Assert.DoesNotThrow(() =>
                           dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath6));
            Assert.DoesNotThrow(() =>
                           dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath7));
            Assert.DoesNotThrow(() =>
                          dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath8));

            model.Open(testPath);
            Assert.AreEqual(8, dynSettings.Controller.CustomNodeManager.LoadedCustomNodes.Count);
            // check all the nodes and connectors are loaded
            Assert.AreEqual(33, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(31, model.CurrentWorkspace.Connectors.Count);

            //cannot run this graph in test yet as it uses python nodes and a360 raas lighting api
            // Assert.DoesNotThrow(() =>dynSettings.Controller.RunExpression(true));

            // assert that references to revit elements are correct, faces, families, etc.

            var workspace = model.CurrentWorkspace;

            var familyTypeSelectNode =
                (FamilyTypeSelector)workspace.Nodes.First(x => x is FamilyTypeSelector);
            // ensure that we have the right window type selected

            var result_item =familyTypeSelectNode.Items[familyTypeSelectNode.SelectedIndex].Name;

            Assert.AreEqual("Fixed_instance_params:36\" x 72\" -instance parms", result_item);

        }



        #region 14 Curves

        [Test]
        public void AllCurveTestModelCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\all curve test model curve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(47, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(61, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        [Test]
        public void AllCurveTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\all curve test.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(33, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(33, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        [Test]
        public void ArcAndLineFromRefPoints()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\Arc and Line from Ref Points.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(15, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        [Test]
        public void ArcAndLine()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\Arc and Line.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(17, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(16, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        [Test]
        public void ArcFromRefPoints()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\Arc from Ref Points.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        [Test]
        public void Arc()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\Arc.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(13, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        [Test]
        public void Circle()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\circle.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(13, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        [Test]
        public void Ellipse()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\ellipse.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(14, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(14, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        #endregion

        #region 06 Python Node

        [Test]
        public void ConnectTwoPointArraysWithoutPython()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\connect two point arrays without python.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        [Ignore]
        public void ConnectTwoPointArrays()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\connect two point arrays.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        [Ignore]
        public void CreateSineWaveFromSelectedCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\create sine wave from selected curve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(2, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }

        [Test]
        public void CreateSineWaveFromSelectedPoints()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Samples\create sine wave from selected points.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

        }



        #endregion
    }
}
