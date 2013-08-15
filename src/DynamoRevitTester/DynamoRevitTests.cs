﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Microsoft.Practices.Prism;
using NUnit.Framework;
using Value = Dynamo.FScheme.Value;

namespace DynamoRevitTests
{
    [TestFixture]
    internal class DynamoRevitTests
    {
        private Transaction _trans;
        private List<Element> _elements = new List<Element>();
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
            FileInfo fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assDir = fi.DirectoryName;
            string testsLoc = Path.Combine(assDir, @"..\..\test\revit\");
            _testPath = Path.GetFullPath(testsLoc);

            //get the samples path
            string samplesLoc = Path.Combine(assDir, @"..\..\doc\distrib\Samples\");
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
            OpenEmptyModel();
        }

        [TearDown]
        //Called after each test method
        public void Cleanup()
        {
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
        public void CanOpenReferencePointTest()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string testPath = Path.Combine(_testPath, "ReferencePointTest.dyn");
            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            Assert.AreEqual(3, dynSettings.Controller.DynamoModel.Nodes.Count());

            dynSettings.Controller.RunCommand(vm.RunExpressionCommand,true);
        }

        [Test]
        public void CreatePointSequenceSample()
        {
            string samplePath = Path.Combine(_samplesPath, @".\01 Create Point\create point_sequence.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void CreatePointEndSample()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_samplesPath, @".\01 Create Point\create point - end.dyn");
            string testPath = Path.GetFullPath(samplePath);

            //test running the expression
            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);

            //test copying and pasting the workflow
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.AddRange(dynSettings.Controller.DynamoModel.Nodes);
            dynSettings.Controller.RunCommand(vm.CopyCommand, null);
            dynSettings.Controller.RunCommand(vm.PasteCommand, null);
        }

        [Test]
        public void CreatePointSample()
        {
            string samplePath = Path.Combine(_samplesPath, @".\01 Create Point\create point.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void RefGridSlidersSample()
        {
            string samplePath = Path.Combine(_samplesPath, @".\02 Ref Grid Sliders\ref grid sliders.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void RefGridSlidersEndSample()
        {
            string samplePath = Path.Combine(_samplesPath, @".\02 Ref Grid Sliders\ref grid sliders - end.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void DivideSelectedCurveEndSample()
        {
            ModelCurve mc1;
            CreateOneModelCurve(out mc1);

            string samplePath = Path.Combine(_samplesPath, @".\03 Divide Selected Curve\divide selected curve - end.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            var selectionNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynCurvesBySelection);
            Assert.AreEqual(1, selectionNodes.Count());

            ((dynCurvesBySelection)selectionNodes.ElementAt(0)).SelectedElement = mc1;

            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void DivideSelectedCurveSample()
        {
            ModelCurve mc1;
            CreateOneModelCurve(out mc1);

            string samplePath = Path.Combine(_samplesPath, @".\03 Divide Selected Curve\divide selected curve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            var selectionNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynCurvesBySelection);
            Assert.AreEqual(1, selectionNodes.Count());

            ((dynCurvesBySelection)selectionNodes.ElementAt(0)).SelectedElement = mc1;

            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void FormFromCurveSelectionListSample()
        {
            ModelCurve mc1;
            ModelCurve mc2;
            CreateTwoModelCurves(out mc1, out mc2);

            string samplePath = Path.Combine(_samplesPath, @".\04 Form From Curve Selection\form from curve selection - list.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            //get the two selection nodes in the sample
            var selectionNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynCurvesBySelection);
            Assert.AreEqual(2, selectionNodes.Count());

            ((dynCurvesBySelection)selectionNodes.ElementAt(0)).SelectedElement = mc1;
            ((dynCurvesBySelection)selectionNodes.ElementAt(1)).SelectedElement = mc2;

            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void FormFromCurveSelectionSample()
        {
            ModelCurve mc1;
            ModelCurve mc2;
            CreateTwoModelCurves(out mc1, out mc2);

            string samplePath = Path.Combine(_samplesPath, @".\04 Form From Curve Selection\form from curve selection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            //populate the selection nodes in the sample
            var selectionNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynCurvesBySelection);
            ((dynCurvesBySelection)selectionNodes.ElementAt(0)).SelectedElement = mc1;
            ((dynCurvesBySelection)selectionNodes.ElementAt(1)).SelectedElement = mc2;

            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void GraphFunctionAndConnectPointsSample()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_samplesPath, @".\05 Graph Function\graph function and connect points.dyn");
            string testPath = Path.GetFullPath(samplePath);

            string customDefPath1 = Path.Combine(_defsPath, "GraphFunction.dyf");
            string customDefPath2 = Path.Combine(_defsPath, "ConnectPoints.dyf");
            Assert.IsTrue(File.Exists(customDefPath1), "Cannot find specified custom definition to load for testing.");
            Assert.IsTrue(File.Exists(customDefPath2), "Cannot find specified custom definition to load for testing.");

            Assert.IsTrue(dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath1) != null);
            Assert.IsTrue(dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath2) != null);

            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);
        }

        [Test]
        public void ScalableGraphFunctionSample()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_samplesPath, @".\05 Graph Function\scalable graph function.dyn");
            string testPath = Path.GetFullPath(samplePath);

            string customDefPath = Path.Combine(_defsPath, "Cf(dx).dyf");
            Assert.IsTrue(File.Exists(customDefPath), "Cannot find specified custom definition to load for testing.");
            Assert.IsTrue(dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath) != null);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void GraphFunctionSample()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_samplesPath, @".\05 Graph Function\graph function.dyn");
            string testPath = Path.GetFullPath(samplePath);

            string customDefPath = Path.Combine(_defsPath, "GraphFunction.dyf");
            Assert.IsTrue(File.Exists(customDefPath), "Cannot find specified custom definition to load for testing.");
            Assert.IsTrue(dynSettings.Controller.CustomNodeManager.AddFileToPath(customDefPath) != null);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void InstParamSample()
        {
            string modelPath = Path.GetFullPath(Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param.rvt"));
            SwapCurrentModel(modelPath);

            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void InstParam2MassesSample()
        {
            string modelPath = Path.GetFullPath(Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param mass families.rvt"));
            SwapCurrentModel(modelPath);

            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param 2 masses.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void InstParam2MassesDrivingEachOtherSample()
        {
            string modelPath = Path.GetFullPath(Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param mass families.rvt"));
            SwapCurrentModel(modelPath);

            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_samplesPath, @".\08 Get Set Family Params\inst param 2 masses driving each other.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
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
            //DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            //string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\connect two point arrays without python.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            //dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
            Assert.Inconclusive("Python examples do not play well with testing.");
        }

        [Test]
        public void ConnectTwoPointArrays()
        {
            //DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            //string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\connect two point arrays.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            //dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
            Assert.Inconclusive("Python examples do not play well with testing.");
        }
   
        [Test]
        public void CreateSineWaveFromSelectedCurve()
        {
            //DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

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
        //    DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

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
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_testPath, @"SelectFamily.dyn");
            string testPath = Path.GetFullPath(samplePath);
            
            //open the test file
            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);

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

            dynFamilyTypeSelector typeSelNode = (dynFamilyTypeSelector)dynSettings.Controller.DynamoModel.Nodes.First();
            Assert.AreEqual(typeSelNode.Items.Count, count);

            //assert that the selected index is correct
            Assert.AreEqual(typeSelNode.SelectedIndex, 3);

            //now try and set the selected index to something
            //greater than what is possible
            typeSelNode.SelectedIndex = count + 5;
            Assert.AreEqual(typeSelNode.SelectedIndex, -1);
        }

        [Test]
        public void ModelCurveNode()
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_testPath, @".\ModelCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);

            var fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(CurveElement));

            //verify one model curve created
            int count = fec.ToElements().Count;
            Assert.IsInstanceOf(typeof(ModelCurve), fec.ToElements().First());
            Assert.AreEqual(1, count);

            ElementId id = fec.ToElements().First().Id;

            //update any number node and verify 
            //that the element gets updated not recreated
            var doubleNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynDoubleInput);
            var node = doubleNodes.First() as dynDoubleInput;

            Assert.IsNotNull(node);

            node.Value = node.Value + .1;
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);
            Assert.AreEqual(1, fec.ToElements().Count);

            Assert.AreEqual(id.IntegerValue, fec.ToElements().First().Id.IntegerValue);
        }

        [Test]
        public void ReferenceCurveNode()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_testPath, @".\ReferenceCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);

            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(CurveElement));

            //verify one model curve created
            int count = fec.ToElements().Count;
            Assert.IsInstanceOf(typeof(ModelCurve), fec.ToElements().First());
            Assert.IsTrue(((ModelCurve)fec.ToElements().First()).IsReferenceLine);
            Assert.AreEqual(1, count);

            ElementId id = fec.ToElements().First().Id;

            //update any number node and verify 
            //that the element gets updated not recreated
            var doubleNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynBasicInteractive<double>);
            dynBasicInteractive<double> node = doubleNodes.First() as dynBasicInteractive<double>;
            node.Value = node.Value + .1;
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);
            Assert.AreEqual(1, fec.ToElements().Count);

            Assert.AreEqual(id.IntegerValue, fec.ToElements().First().Id.IntegerValue);
        }

        [Test]
        public void LoftNode()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_testPath, @".\Loft.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);

            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(GenericForm));

            //verify one loft created
            int count = fec.ToElements().Count;
            Assert.IsInstanceOf(typeof(Form), fec.ToElements().First());
            Assert.AreEqual(1, count);
        }

        [Test]
        public void CurveByPointsNode()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_testPath, @".\CurveByPoints.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);

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

            var ptSelectNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynPointBySelection);
            if (!ptSelectNodes.Any())
                Assert.Fail("Could not find point selection nodes in dynamo graph.");

            ((dynPointBySelection)ptSelectNodes.ElementAt(0)).SelectedElement = p1;
            ((dynPointBySelection)ptSelectNodes.ElementAt(1)).SelectedElement = p2;
            ((dynPointBySelection)ptSelectNodes.ElementAt(2)).SelectedElement = p3;
            ((dynPointBySelection)ptSelectNodes.ElementAt(3)).SelectedElement = p4;

            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);

            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(CurveElement));

            Assert.AreEqual(fec.ToElements().Count(), 1);

            CurveByPoints mc = (CurveByPoints)fec.ToElements().ElementAt(0);
            Assert.IsTrue(mc.IsReferenceLine);

            //now flip the switch for creating a reference curve
            var boolNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynBoolSelector).First();

            ((dynBasicInteractive<bool>)boolNode).Value = false;

            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);
            Assert.AreEqual(fec.ToElements().Count(), 1);

            mc = (CurveByPoints)fec.ToElements().ElementAt(0);
            Assert.IsFalse(mc.IsReferenceLine);
        }

        [Test]
        public void XYZFromReferencePointNode()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_testPath, @".\XYZFromReferencePoint.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);
            //dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);
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
            var node = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynXYZFromReferencePoint).First();

            Value v = ((dynNodeWithOneOutput)node).Evaluate(args);
            Assert.IsInstanceOf(typeof(XYZ), ((Value.Container)v).Item);
        }

        [Test]
        public void CurveByPointsByLineNode()
        {
            //this sample creates a geometric line
            //then creates a curve by points from that line
   
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_testPath, @".\CurveByPointsByLine.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);

            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));

            Assert.AreEqual(2, fec.ToElements().Count());

            //now change one of the number inputs and rerun
            //verify that there are still only two reference points in
            //the model
            var node = dynSettings.Controller.DynamoModel.Nodes.OfType<dynDoubleInput>().First();
            node.Value = "12.0";

            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);
            fec = null;
            fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(2, fec.ToElements().Count);
        }

        [Test]
        public void BlendSolid()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_testPath, @".\BlendSolid.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);

            var blendNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is CreateBlendGeometry).First();
            dynSolidBase nodeAsSolidBase = (dynSolidBase)blendNode;
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
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_testPath, @".\RevolveSolid.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);

            var revolveNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is CreateRevolvedGeometry).First();
            dynSolidBase nodeAsSolidBase = (dynSolidBase)revolveNode;
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
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_testPath, @".\SweepToMakeSolid.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);

            var sweepNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is CreateSweptGeometry).First();
            dynSolidBase nodeAsSolidBase = (dynSolidBase)sweepNode;
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
                 DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

                 string samplePath = Path.Combine(_testPath, @".\SolidBySkeleton.dyn");
                 string testPath = Path.GetFullPath(samplePath);

                 dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);
                 dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);

                 var skeletonNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynSkinCurveLoops).First();
                 dynSolidBase nodeAsSolidBase = (dynSolidBase)skeletonNode;
                 Solid result = nodeAsSolidBase.resultingSolidForTestRun().First();
                 double volumeMin = 82500.0;
                 double volumeMax = 84500.0;
                 double actualVolume = result.Volume;
                 Assert.Greater(actualVolume, volumeMin);
                 Assert.Less(actualVolume, volumeMax);
             }
        }

        [Test]
        public void ClosedCurveTest()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_testPath, @".\ClosedCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);

            var extrudeNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is CreateExtrusionGeometry).First();
            dynSolidBase nodeAsSolidBase = (dynSolidBase)extrudeNode;
            Solid result = nodeAsSolidBase.resultingSolidForTestRun().First();
            double volumeMin = 3850;
            double volumeMax = 4050;
            double actualVolume = result.Volume;
            Assert.Greater(actualVolume, volumeMin);
            Assert.Less(actualVolume, volumeMax);
        }

        [Test]
        public void AdaptiveComponentsNode()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string path = Path.Combine(_testPath, @".\AdaptiveComponents\AdaptiveComponentSample.rfa");
            string modelPath = Path.GetFullPath(path);
            SwapCurrentModel(modelPath);

            string samplePath = Path.Combine(_testPath, @".\AdaptiveComponents\AdaptiveComponents.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);

            //the .dyn has the slider set at 5. let's make sure that
            //if you set the slider to something else before running, that it get the correct number
            var slider = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynDoubleSliderInput).First();
            ((dynBasicInteractive<double>)slider).Value = 1;

            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);

            //get all the family instances in the document
            var acs = GetAllFamilyInstancesWithTypeName("3PointAC_wireTruss");
            Assert.AreEqual(1, acs.Count());

            //change the number slider
            ((dynBasicInteractive<double>)slider).Value = 3;

            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);
            acs = GetAllFamilyInstancesWithTypeName("3PointAC_wireTruss");
            Assert.AreEqual(3, acs.Count());

        }

        [Test]
        public void SwitchDocuments()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            //open the workflow and run the expression
            string testPath = Path.Combine(_testPath, "ReferencePointTest.dyn");
            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);
            Assert.AreEqual(3, dynSettings.Controller.DynamoModel.Nodes.Count());
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);

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
            var doubleNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynBasicInteractive<double>);
            dynBasicInteractive<double> node = doubleNodes.First() as dynBasicInteractive<double>;
            node.Value = node.Value + .1;

            //run the expression again
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);
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
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_testPath, @".\LacingTest.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);

            var xyzNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynXYZ).First();
            Assert.IsNotNull(xyzNode);

            //test the first lacing
            xyzNode.ArgumentLacing = LacingStrategy.First;
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);

            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(1, fec.ToElements().Count());

            //test the shortest lacing
            xyzNode.ArgumentLacing = LacingStrategy.First;
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);
            fec = null;
            fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(1, fec.ToElements().Count());

            //test the longest lacing
            xyzNode.ArgumentLacing = LacingStrategy.Longest;
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);
            fec = null;
            fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(5, fec.ToElements().Count());

            //test the cross product lacing
            xyzNode.ArgumentLacing = LacingStrategy.CrossProduct;
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);
            fec = null;
            fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(20, fec.ToElements().Count());
        }

        [Test]
        public void DividedSurface()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_testPath, @".\DividedSurfaceTest.dyn");
            string testPath = Path.GetFullPath(samplePath);

            var shellPath = Path.Combine(_testPath, "shell.rfa");

            SwapCurrentModel(shellPath);

            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);

            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof (DividedSurface));

            //did it create a divided surface?
            Assert.AreEqual(1, fec.ToElements().Count());

            var ds = (DividedSurface) fec.ToElements()[0];
            Assert.AreEqual(5, ds.USpacingRule.Number);
            Assert.AreEqual(5, ds.VSpacingRule.Number);

            //can we change the number of divisions
            var numNode = dynSettings.Controller.DynamoModel.Nodes.OfType<dynDoubleInput>().First();
            numNode.Value = "10";
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);

            //did it create a divided surface?
            Assert.AreEqual(10, ds.USpacingRule.Number);
            Assert.AreEqual(10, ds.VSpacingRule.Number);

            //does it throw an error when we try to set a negative number of divisions
            numNode.Value = "-5";
            Assert.Throws(typeof(AssertionException),
                          () => dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true));
            
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

            dynRevitSettings.Revit.OpenAndActivateDocument(_emptyModelPath);
            empty1.Document.Close(false);
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
            foreach (FileInfo file in di.GetFiles())
            {
                if(file.Extension != ".dyn")
                    continue;
                dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(file.FullName);
                dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
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
