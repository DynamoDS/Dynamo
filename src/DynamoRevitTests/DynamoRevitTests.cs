using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Microsoft.Practices.Prism;
using NUnit.Framework;
using CurveByPoints = Autodesk.Revit.DB.CurveByPoints;
using DividedSurface = Autodesk.Revit.DB.DividedSurface;
using ModelCurve = Autodesk.Revit.DB.ModelCurve;
using Plane = Autodesk.Revit.DB.Plane;
using SketchPlane = Autodesk.Revit.DB.SketchPlane;
using Transaction = Autodesk.Revit.DB.Transaction;
using Value = Dynamo.FScheme.Value;

namespace DynamoRevitTests
{
    [TestFixture]
    internal class DynamoRevitTests
    {
        private Transaction _trans;
        private string _testPath;
        private string _samplesPath;
        private string _defsPath;
        private string _emptyModelPath;
        private string _emptyModelPath1;

        [TestFixtureSetUp]
        public void InitFixture()
        {
            
        }

        [TestFixtureTearDown]
        public void CleanupFixture()
        {
        }

        [SetUp]
        //Called before each test method
        public void Init()
        {
            //it doesn't make sense to do these steps before every test
            //but when running from the revit plugin we are not loading the 
            //fixture, so the initfixture method is not called.

            //get the test path
            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assDir = fi.DirectoryName;
            string testsLoc = Path.Combine(assDir, @"..\..\..\test\revit\");
            _testPath = Path.GetFullPath(testsLoc);

            //get the samples path
            string samplesLoc = Path.Combine(assDir, @"..\..\..\doc\distrib\Samples\");
            _samplesPath = Path.GetFullPath(samplesLoc);

            //set the custom node loader search path
            string defsLoc = Path.Combine(assDir, @".\definitions\");
            _defsPath = Path.GetFullPath(defsLoc);

            _emptyModelPath = Path.Combine(_testPath, "empty.rfa");

            if (dynRevitSettings.Revit.Application.VersionNumber.Contains("2014") &&
                dynRevitSettings.Revit.Application.VersionName.Contains("Vasari"))
            {
                _emptyModelPath = Path.Combine(_testPath, "emptyV.rfa");
                _emptyModelPath1 = Path.Combine(_testPath, "emptyV1.rfa");
            }
            else
            {
                _emptyModelPath = Path.Combine(_testPath, "empty.rfa");
                _emptyModelPath1 = Path.Combine(_testPath, "empty1.rfa");
            }
            
            /*
            _emptyModelPath = Path.Combine(_testPath, "empty.rfa");
            _emptyModelPath1 = Path.Combine(_testPath, "empty1.rfa");
            */
            //open an empty model before every test
            //OpenEmptyModel();
        }

        [TearDown]
        //Called after each test method
        public void Cleanup()
        {
            // opens an empty model and closes
            // the current model without saving 
            OpenEmptyModel();

        }

        [Test]
        public void CanCreateAndDeleteAReferencePoint()
        {
            using (_trans = _trans = new Transaction(dynRevitSettings.Doc.Document, "CreateAndDeleteAreReferencePoint"))
            {
                _trans.Start();

                FailureHandlingOptions fails = _trans.GetFailureHandlingOptions();
                fails.SetClearAfterRollback(true);
                _trans.SetFailureHandlingOptions(fails);

                ReferencePoint rp = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ());

                //make a filter for reference points.
                ElementClassFilter ef = new ElementClassFilter(typeof(ReferencePoint));
                FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
                fec.WherePasses(ef);
                Assert.AreEqual(1, fec.ToElements().Count());

                dynRevitSettings.Doc.Document.Delete(rp);
                _trans.Commit();
            }
        }

        [Test]
        public void ReferencePoint()
        {
            var model = dynSettings.Controller.DynamoModel;

            string testPath = Path.Combine(_testPath, "ReferencePoint.dyn");
            model.Open(testPath);
            Assert.AreEqual(3, dynSettings.Controller.DynamoModel.Nodes.Count());
            
            dynSettings.Controller.RunExpression(true);
        }

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

        #region Python samples
        
        [Test]
        public void ConnectTwoPointArraysWithoutPython()
        {
            //var model = dynSettings.Controller.DynamoModel;

            //string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\connect two point arrays without python.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            //dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
            Assert.Inconclusive("Python examples do not play well with testing.");
        }

        [Test]
        public void ConnectTwoPointArrays()
        {
            //var model = dynSettings.Controller.DynamoModel;

            //string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\connect two point arrays.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            //dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
            Assert.Inconclusive("Python examples do not play well with testing.");
        }
   
        [Test]
        public void CreateSineWaveFromSelectedCurve()
        {
            //var model = dynSettings.Controller.DynamoModel;

            //string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\create sine wave from selected curve.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //CurveByPoints cbp = null;
            //using (_trans = new Transaction(dynRevitSettings.Doc.Document))
            //{
            //    _trans.Start("Create reference points for testing Python node.");

            //    ReferencePoint p1 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ());
            //    ReferencePoint p2 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ(0,10,0));
            //    ReferencePoint p3 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ(0,20,0));
            //    ReferencePointArray ptArr = new ReferencePointArray();
            //    ptArr.Append(p1);
            //    ptArr.Append(p2);
            //    ptArr.Append(p3);

            //    cbp = dynRevitSettings.Doc.Document.FamilyCreate.NewCurveByPoints(ptArr);

            //    _trans.Commit();
            //}

            //Assert.IsNotNull(cbp);

            //dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            //var selectionNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynCurvesBySelection).First();
            //((dynCurvesBySelection)selectionNode).SelectedElement = cbp;

            ////delete the transaction node when testing
            ////var transNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynTransaction).First();
            ////dynRevitSettings.Controller.RunCommand(vm.DeleteCommand, transNode);

            //dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);

            Assert.Inconclusive("Python examples do not play well with testing.");
        }

        //[Test]
        //public void CreateSineWaveFromSelectedPoints()
        //{
        //    var model = dynSettings.Controller.DynamoModel;

        //    string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\create sine wave from selected points.dyn");
        //    string testPath = Path.GetFullPath(samplePath);

        //    ReferencePoint p1 = null;
        //    ReferencePoint p2 = null;

        //    using (_trans = new Transaction(dynRevitSettings.Doc.Document))
        //    {
        //        _trans.Start("Create reference points for testing python node.");

        //        p1 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ());
        //        p2 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ(0, 10, 0));

        //        _trans.Commit();
        //    }

        //    dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);

        //    var selectionNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynPointBySelection);
        //    Assert.AreEqual(2, selectionNodes.Count());

        //    ((dynPointBySelection)selectionNodes.ElementAt(0)).SelectedElement = p1;
        //    ((dynPointBySelection)selectionNodes.ElementAt(1)).SelectedElement = p2;

        //    //delete the transaction node when testing
        //    //var transNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynTransaction).First();
        //    //dynRevitSettings.Controller.RunCommand(vm.DeleteCommand, transNode);

        //    dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        //}
        
        #endregion

        [Test]
        public void FamilyTypeSelectorNode()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @"SelectFamily.dyn");
            string testPath = Path.GetFullPath(samplePath);
            
            //open the test file
            model.Open(testPath);

            //first assert that we have only one node
            var nodeCount = dynSettings.Controller.DynamoModel.Nodes.Count;
            Assert.AreEqual(1, nodeCount);

            //assert that we have the right number of family symbols
            //in the node's items source
            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(Family));
            int count = 0;
            foreach (Family f in fec.ToElements())
            {
                foreach (FamilySymbol fs in f.Symbols)
                {
                    count++;
                }
            }

            FamilyTypeSelector typeSelNode = (FamilyTypeSelector)dynSettings.Controller.DynamoModel.Nodes.First();
            Assert.AreEqual(typeSelNode.Items.Count, count);

            //assert that the selected index is correct
            Assert.AreEqual(typeSelNode.SelectedIndex, 3);

            //now try and set the selected index to something
            //greater than what is possible
            typeSelNode.SelectedIndex = count + 5;
            Assert.AreEqual(typeSelNode.SelectedIndex, -1);
        }

        [Test]
        public void ModelCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\ModelCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            var fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(CurveElement));

            //verify five model curves created
            int count = fec.ToElements().Count;
            Assert.IsInstanceOf(typeof(ModelCurve), fec.ToElements().First());
            Assert.AreEqual(5, count);

            ElementId id = fec.ToElements().First().Id;

            //update any number node and verify 
            //that the element gets updated not recreated
            var doubleNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is DoubleInput);
            var node = doubleNodes.First() as DoubleInput;

            Assert.IsNotNull(node);

            node.Value = node.Value + .1;
            dynSettings.Controller.RunExpression(true);
            Assert.AreEqual(5, fec.ToElements().Count);
        }

        [Test]
        public void ReferenceCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\ReferenceCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(CurveElement));

            //verify five model curves created
            int count = fec.ToElements().Count;
            Assert.IsInstanceOf(typeof(ModelCurve), fec.ToElements().First());
            Assert.IsTrue(((ModelCurve)fec.ToElements().First()).IsReferenceLine);
            Assert.AreEqual(5, count);

            ElementId id = fec.ToElements().First().Id;

            //update any number node and verify 
            //that the element gets updated not recreated
            var doubleNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is DoubleInput);
            var node = doubleNodes.First() as DoubleInput;

            Assert.IsNotNull(node);

            node.Value = node.Value + .1;
            dynSettings.Controller.RunExpression(true);
            Assert.AreEqual(5, fec.ToElements().Count);
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
        public void CurveByPoints()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\CurveByPoints.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            //cerate some points and wire them
            //to the selections
            ReferencePoint p1, p2, p3, p4;

            using(_trans = new Transaction(dynRevitSettings.Doc.Document))
            {
                _trans.Start("Create reference points for testing.");

                p1 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ(1, 5, 12));
                p2 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ(5, 1, 12));
                p3 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ(12, 1, 5));
                p4 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ(5, 12, 1));

                _trans.Commit();
            }

            var ptSelectNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is PointBySelection);
            if (!ptSelectNodes.Any())
                Assert.Fail("Could not find point selection nodes in dynamo graph.");

            ((PointBySelection)ptSelectNodes.ElementAt(0)).SelectedElement = p1;
            ((PointBySelection)ptSelectNodes.ElementAt(1)).SelectedElement = p2;
            ((PointBySelection)ptSelectNodes.ElementAt(2)).SelectedElement = p3;
            ((PointBySelection)ptSelectNodes.ElementAt(3)).SelectedElement = p4;

            dynSettings.Controller.RunExpression(true);

            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(CurveElement));

            Assert.AreEqual(fec.ToElements().Count(), 1);

            CurveByPoints mc = (CurveByPoints)fec.ToElements().ElementAt(0);
            Assert.IsTrue(mc.IsReferenceLine);

            //now flip the switch for creating a reference curve
            var boolNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is BoolSelector).First();

            ((BasicInteractive<bool>)boolNode).Value = false;

            dynSettings.Controller.RunExpression(true);
            Assert.AreEqual(fec.ToElements().Count(), 1);

            mc = (CurveByPoints)fec.ToElements().ElementAt(0);
            Assert.IsFalse(mc.IsReferenceLine);
        }

        [Test]
        public void XYZFromReferencePoint()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\XYZFromReferencePoint.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            ReferencePoint rp;
            using(_trans = new Transaction(dynRevitSettings.Doc.Document))
            {
                _trans.Start("Create a reference point.");

                rp = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ());

                _trans.Commit();

            }
            FSharpList<Value> args = FSharpList<Value>.Empty;
            args = FSharpList<Value>.Cons(Value.NewContainer(rp), args);

            //find the XYZFromReferencePoint node
            var node = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is XyzFromReferencePoint).First();

            Value v = ((NodeWithOneOutput)node).Evaluate(args);
            Assert.IsInstanceOf(typeof(XYZ), ((Value.Container)v).Item);
        }

        [Test]
        public void CurveByPointsByLineNode()
        {
            //this sample creates a geometric line
            //then creates a curve by points from that line

            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\CurveByPointsByLine.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));

            Assert.AreEqual(2, fec.ToElements().Count());

            //now change one of the number inputs and rerun
            //verify that there are still only two reference points in
            //the model
            var node = dynSettings.Controller.DynamoModel.Nodes.OfType<DoubleInput>().First();
            node.Value = "12.0";

            dynSettings.Controller.RunExpression(true);

            fec = null;
            fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(2, fec.ToElements().Count);
        }

        [Test]
        public void BlendSolid()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\BlendSolid.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            var blendNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is CreateBlendGeometry).First();
            SolidBase nodeAsSolidBase = (SolidBase)blendNode;
            Solid result = nodeAsSolidBase.resultingSolidForTestRun().First();
            double volumeMin = 3700000.0;
            double volumeMax = 3900000.0;
            double actualVolume = result.Volume;
            Assert.Greater(actualVolume, volumeMin);
            Assert.Less(actualVolume, volumeMax);
        }

        [Test]
        public void RevolveSolid()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\RevolveSolid.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            var revolveNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is CreateRevolvedGeometry).First();
            SolidBase nodeAsSolidBase = (SolidBase)revolveNode;
            Solid result = nodeAsSolidBase.resultingSolidForTestRun().First();
            double volumeMin = 13300.0;
            double volumeMax = 13550.0;
            double actualVolume = result.Volume;
            Assert.Greater(actualVolume, volumeMin);
            Assert.Less(actualVolume, volumeMax);
        }

        [Test]
        public void SweepToMakeSolid()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\SweepToMakeSolid.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            var sweepNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is CreateSweptGeometry).First();
            SolidBase nodeAsSolidBase = (SolidBase)sweepNode;
            Solid result = nodeAsSolidBase.resultingSolidForTestRun().First();
            double volumeMin = 11800.0;
            double volumeMax = 12150.0;
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

                 var skeletonNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is SkinCurveLoops).First();
                 SolidBase nodeAsSolidBase = (SolidBase)skeletonNode;
                 Solid result = nodeAsSolidBase.resultingSolidForTestRun().First();
                 double volumeMin = 82500.0;
                 double volumeMax = 84500.0;
                 double actualVolume = result.Volume;
                 Assert.Greater(actualVolume, volumeMin);
                 Assert.Less(actualVolume, volumeMax);
             }
        }

        [Test]
        public void ClosedCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\ClosedCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            var extrudeNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is CreateExtrusionGeometry).First();
            SolidBase nodeAsSolidBase = (SolidBase)extrudeNode;
            Solid result = nodeAsSolidBase.resultingSolidForTestRun().First();
            double volumeMin = 3850;
            double volumeMax = 4050;
            double actualVolume = result.Volume;
            Assert.Greater(actualVolume, volumeMin);
            Assert.Less(actualVolume, volumeMax);
        }

        [Test]
        public void AdaptiveComponent()
        {
            var model = dynSettings.Controller.DynamoModel;

            //string path = Path.Combine(_testPath, @".\AdaptiveComponent.rfa");
            //string modelPath = Path.GetFullPath(path);
            //SwapCurrentModel(modelPath);

            string samplePath = Path.Combine(_testPath, @".\AdaptiveComponent.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            //the .dyn has the slider set at 5. let's make sure that
            //if you set the slider to something else before running, that it get the correct number
            var slider = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is DoubleSliderInput).First();
            ((BasicInteractive<double>)slider).Value = 1;

            dynSettings.Controller.RunExpression(true);

            //get all the family instances in the document
            var acs = GetAllFamilyInstancesWithTypeName("3PointAC_wireTruss");
            Assert.AreEqual(1, acs.Count());

            //change the number slider
            ((BasicInteractive<double>)slider).Value = 3;

            dynSettings.Controller.RunExpression(true);
            acs = GetAllFamilyInstancesWithTypeName("3PointAC_wireTruss");
            Assert.AreEqual(3, acs.Count());

        }

        [Test]
        public void SwitchDocuments()
        {
            var model = dynSettings.Controller.DynamoModel;

            //open the workflow and run the expression
            string testPath = Path.Combine(_testPath, "ReferencePointTest.dyn");
            model.Open(testPath);
            Assert.AreEqual(3, dynSettings.Controller.DynamoModel.Nodes.Count());
            dynSettings.Controller.RunExpression(true);

            //verify we have a reference point
            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(1, fec.ToElements().Count());

            //open a new document and activate it
            UIDocument initialDoc = dynRevitSettings.Revit.ActiveUIDocument;
            string shellPath = Path.Combine(_testPath, @"empty1.rfa");
            dynRevitSettings.Revit.OpenAndActivateDocument(shellPath);
            initialDoc.Document.Close(false);

            //assert that the doc is set on the controller
            Assert.IsNotNull(dynRevitSettings.Doc.Document);

            //update the double node so the graph reevaluates
            var doubleNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is BasicInteractive<double>);
            BasicInteractive<double> node = doubleNodes.First() as BasicInteractive<double>;
            node.Value = node.Value + .1;

            //run the expression again
            dynSettings.Controller.RunExpression(true);
            fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(1, fec.ToElements().Count());

            //finish out by restoring the original
            initialDoc = dynRevitSettings.Revit.ActiveUIDocument;
            shellPath = Path.Combine(_testPath, @"empty.rfa");
            dynRevitSettings.Revit.OpenAndActivateDocument(shellPath);
            initialDoc.Document.Close(false);

        }

        [Test]
        public void CanChangeLacingAndHaveElementsUpdate()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\LacingTest.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            var xyzNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is Xyz).First();
            Assert.IsNotNull(xyzNode);

            //test the first lacing
            xyzNode.ArgumentLacing = LacingStrategy.First;
            dynSettings.Controller.RunExpression(true);

            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(1, fec.ToElements().Count());

            //test the shortest lacing
            xyzNode.ArgumentLacing = LacingStrategy.First;
            dynSettings.Controller.RunExpression(true);
            fec = null;
            fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(1, fec.ToElements().Count());

            //test the longest lacing
            xyzNode.ArgumentLacing = LacingStrategy.Longest;
            dynSettings.Controller.RunExpression(true);
            fec = null;
            fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(5, fec.ToElements().Count());

            //test the cross product lacing
            xyzNode.ArgumentLacing = LacingStrategy.CrossProduct;
            dynSettings.Controller.RunExpression(true);
            fec = null;
            fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(20, fec.ToElements().Count());
        }

        [Test]
        public void DividedSurface()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\DividedSurface.dyn");
            string testPath = Path.GetFullPath(samplePath);

            //var shellPath = Path.Combine(_testPath, "shell.rfa");
            //SwapCurrentModel(shellPath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof (DividedSurface));

            //did it create a divided surface?
            Assert.AreEqual(1, fec.ToElements().Count());

            var ds = (DividedSurface) fec.ToElements()[0];
            Assert.AreEqual(5, ds.USpacingRule.Number);
            Assert.AreEqual(5, ds.VSpacingRule.Number);

            //can we change the number of divisions
            var numNode = dynSettings.Controller.DynamoModel.Nodes.OfType<DoubleInput>().First();
            numNode.Value = "10";
            dynSettings.Controller.RunExpression(true);

            //did it create a divided surface?
            Assert.AreEqual(10, ds.USpacingRule.Number);
            Assert.AreEqual(10, ds.VSpacingRule.Number);

            //does it throw an error when we try to set a negative number of divisions
            numNode.Value = "-5";
            Assert.Throws(typeof(AssertionException),
                          () => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void CurveLoop()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\CurveLoop.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void UVRandom()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\UVRandom.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void AdaptiveComponentByFace()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\AdaptiveComponentByFace.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void AdaptiveComponentByCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\AdaptiveComponentByCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void CurveCurveIntersection()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\CurveCurveIntersection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void CurveFaceIntersection()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\CurveFaceIntersection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void FaceFaceIntersection()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\FaceFaceIntersection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void GetCurveDomain()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\GetCurveDomain.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void GetSurfaceDomain()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\GetSurfaceDomain.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void OffsetCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\OffsetCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void ThickenCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\ThickenCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void GetMaterialByName()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\GetMaterialByName.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        /// <summary>
        /// Sanity Check should always throw an error.
        /// </summary>
        [Test]
        public void SanityCheck()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\SanityCheck.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.Throws(typeof(AssertionException), () => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void Length()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Length.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void MAGN_66()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\MAGN_66.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }
        
        [Test]
        public void CurvebyPointsArc()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\CurvebyPointsArc.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
            
            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(CurveElement));

            Assert.AreEqual(fec.ToElements().Count(), 1);

            CurveByPoints mc = (CurveByPoints)fec.ToElements().ElementAt(0);
        }
        
        [Test]
        public void CurvebyPointsEllipse()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\CurvebyPointsEllipse.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
            
            
            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(CurveElement));

            Assert.AreEqual(fec.ToElements().Count(), 1);

            CurveByPoints mc = (CurveByPoints)fec.ToElements().ElementAt(0);
        }

        [Test]
        public void ModelText()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\ModelText.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void AxonometricView()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\AxonometricView.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void PerspectiveView()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\PerspectiveView.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        /// <summary>
        /// Automated creation of regression test cases.
        /// </summary>
        /// <param name="dynamoFilePath">The path of the dynamo workspace.</param>
        /// <param name="revitFilePath">The path of the Revit rfa or rvt file.</param>
        [Test, TestCaseSource("SetupRevitRegressionTests")]
        public void Regressions(string dynamoFilePath, string revitFilePath)
        {
            //ensure that the incoming arguments are not empty or null
            //if a dyn file is found in the regression tests directory
            //and there is no corresponding rfa or rvt, then an empty string
            //or a null will be passed into here.
            Assert.IsNotNullOrEmpty(dynamoFilePath, "Dynamo file path is invalid or missing.");
            Assert.IsNotNullOrEmpty(revitFilePath, "Revit file path is invalid or missing.");

            //open the revit model
            SwapCurrentModel(revitFilePath);

            //open the dyn file
            Assert.True(dynSettings.Controller.DynamoViewModel.OpenCommand.CanExecute(dynamoFilePath));
            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(dynamoFilePath);

            //run the expression and assert that it does not
            //throw an error
            Assert.DoesNotThrow(()=> dynSettings.Controller.RunExpression(false));
        }

        /// <summary>
        /// Method referenced by the automated regression testing setup method.
        /// Populates the test cases based on file pairings in the regression tests folder.
        /// </summary>
        /// <returns></returns>
        static List<object[]> SetupRevitRegressionTests()
        {
            var testParameters = new List<object[]>();

            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assDir = fi.DirectoryName;
            string testsLoc = Path.Combine(assDir, @"..\..\test\revit\regression\");
            var regTestPath = Path.GetFullPath(testsLoc);

            var di = new DirectoryInfo(regTestPath);
            var dyns = di.GetFiles("*.dyn");
            foreach (var fileInfo in dyns)
            {
                var data = new object[2];
                data[0] = fileInfo.FullName;

                //find the corresponding rfa or rvt file
                var nameBase = fileInfo.FullName.Remove(fileInfo.FullName.Length - 4);
                var rvt = nameBase + ".rvt";
                var rfa = nameBase + ".rfa";

                //add test parameters for rvt, rfa, or both
                if (File.Exists(rvt))
                {
                    data[1] = rvt;
                }
                
                if (File.Exists(rfa))
                {
                    data[1] = rfa;
                }

                testParameters.Add(data);
            }

            return testParameters;
        }

        /// <summary>
        /// Opens and activates a new model, and closes the old model.
        /// </summary>
        private void SwapCurrentModel(string modelPath)
        {
            Document initialDoc = dynRevitSettings.Doc.Document;
            dynRevitSettings.Revit.OpenAndActivateDocument(modelPath);
            initialDoc.Close(false);
        }

        private void OpenEmptyModel()
        {
            Document initialDoc = dynRevitSettings.Doc.Document;
            UIDocument empty1 = dynRevitSettings.Revit.OpenAndActivateDocument(_emptyModelPath1);
            initialDoc.Close(false);

            // this was used in the previous incarnation of the revit tester
            // it acted as a document swap in the case that the document you were
            // testing on was one of the default 'empty' documents
            // it is removed for now because as tests are called from the journal file
            // they will specify a file to open
            //dynRevitSettings.Revit.OpenAndActivateDocument(_emptyModelPath);
            //empty1.Document.Close(false);
        }

        /// <summary>
        /// Retrieves all family instances of the named type from the active document.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private static IEnumerable<FamilyInstance> GetAllFamilyInstancesWithTypeName(string typeName)
        {
            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(FamilyInstance));
            return fec.ToElements().Where(x => ((FamilyInstance)x).Symbol.Name == typeName).Cast<FamilyInstance>();
        }

        private static void OpenAllSamplesInDirectory(DirectoryInfo di)
        {
            var model = dynSettings.Controller.DynamoModel;

            foreach (FileInfo file in di.GetFiles())
            {
                if(file.Extension != ".dyn")
                    continue;
                
                model.Open(file.FullName); 
                dynSettings.Controller.RunExpression(true);

            }

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                OpenAllSamplesInDirectory(dir);
            }
        }

        /// <summary>
        /// Creates two model curves separated in Z.
        /// </summary>
        /// <param name="mc1"></param>
        /// <param name="mc2"></param>
        private void CreateTwoModelCurves(out ModelCurve mc1, out ModelCurve mc2)
        {
            //create two model curves 
            using (_trans = _trans = new Transaction(dynRevitSettings.Doc.Document, "CreateTwoModelCurves"))
            {
                _trans.Start();

                Plane p1 = new Plane(XYZ.BasisZ, XYZ.Zero);
                Plane p2 = new Plane(XYZ.BasisZ, new XYZ(0, 0, 5));

                SketchPlane sp1 = dynRevitSettings.Doc.Document.FamilyCreate.NewSketchPlane(p1);
                SketchPlane sp2 = dynRevitSettings.Doc.Document.FamilyCreate.NewSketchPlane(p2);
                Curve c1 = dynRevitSettings.Revit.Application.Create.NewLineBound(XYZ.Zero, new XYZ(1, 0, 0));
                Curve c2 = dynRevitSettings.Revit.Application.Create.NewLineBound(new XYZ(0, 0, 5), new XYZ(1, 0, 5));
                mc1 = dynRevitSettings.Doc.Document.FamilyCreate.NewModelCurve(c1, sp1);
                mc2 = dynRevitSettings.Doc.Document.FamilyCreate.NewModelCurve(c2, sp2);

                _trans.Commit();
            }
        }

        /// <summary>
        /// Creates one model curve on a plane with an origin at 0,0,0
        /// </summary>
        /// <param name="mc1"></param>
        private void CreateOneModelCurve(out ModelCurve mc1)
        {
            //create two model curves 
            using (_trans = _trans = new Transaction(dynRevitSettings.Doc.Document, "CreateTwoModelCurves"))
            {
                _trans.Start();

                Plane p1 = new Plane(XYZ.BasisZ, XYZ.Zero);

                SketchPlane sp1 = dynRevitSettings.Doc.Document.FamilyCreate.NewSketchPlane(p1);
                Curve c1 = dynRevitSettings.Revit.Application.Create.NewLineBound(XYZ.Zero, new XYZ(1, 0, 0));
                mc1 = dynRevitSettings.Doc.Document.FamilyCreate.NewModelCurve(c1, sp1);

                _trans.Commit();
            }
        }
    }
}
