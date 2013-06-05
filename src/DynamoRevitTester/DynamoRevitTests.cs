using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Microsoft.Practices.Prism;
using NUnit.Core;
using NUnit.Framework;
using Value = Dynamo.FScheme.Value;

namespace DynamoRevitTests
{
    [TestFixture]
    internal class DynamoRevitTests
    {
        Transaction _trans;
        List<Element> _elements = new List<Element>();
        string _testPath;
        string _samplesPath;
        string _defsPath;
        string _emptyModelPath;

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
        }

        [TearDown]
        //Called after each test method
        public void Cleanup()
        {
            //dynRevitSettings.Controller.ShutDown();

            //delete all the elements in the document
            using (_trans = _trans = new Transaction(dynRevitSettings.Doc.Document))
            {
                _trans.Start("Cleanup test geometry.");

                //get all the generic forms and dissolve them
                //if you don't dissolve them, you don't get the original
                //points back
                FilteredElementCollector fecForms = new FilteredElementCollector(dynRevitSettings.Doc.Document);
                fecForms.OfClass(typeof(GenericForm));

                if (FormUtils.CanBeDissolved(dynRevitSettings.Doc.Document, fecForms.ToElementIds()))
                {
                    FormUtils.DissolveForms(dynRevitSettings.Doc.Document, fecForms.ToElementIds());
                }

                //TODO: can we reset the collector instead of 
                //instantiating anew each time?
                //this is the only way I could get this to work so
                //that it would allow deletion of things like curves with sub-points

                FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
                //delete curves
                fec.OfClass(typeof(CurveElement));
                IList<Element> elements = fec.ToElements();
                if (elements.Count > 0)
                    DynamoLogger.Instance.Log(string.Format("Cleaning up {0} curve elements.", elements.Count));

                for (int i = elements.Count-1; i >= 0; i--)
                {
                    dynRevitSettings.Doc.Document.Delete(elements[i]);
                }

                fec = null;
                fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
                //delete ref points
                elements.Clear();
                fec.OfClass(typeof(ReferencePoint));
                elements = fec.ToElements();
                if (elements.Count > 0)
                    DynamoLogger.Instance.Log(string.Format("Cleaning up {0} reference points.", elements.Count));

                for (int i = elements.Count - 1; i >= 0; i--)
                {
                    dynRevitSettings.Doc.Document.Delete(elements[i]);
                }

                fec = null;
                fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
                //delete forms
                elements.Clear();
                fec.OfClass(typeof(GenericForm));
                elements = fec.ToElements();
                if (elements.Count > 0)
                    DynamoLogger.Instance.Log(string.Format("Cleaning up {0} generic forms.", elements.Count));

                for (int i = elements.Count - 1; i >= 0; i--)
                {
                    dynRevitSettings.Doc.Document.Delete(elements[i]);
                }

                _trans.Commit();
            }
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

            Assert.IsTrue(dynSettings.Controller.CustomNodeLoader.AddFileToPath(Path.Combine(_defsPath, "GraphFunction.dyf")));
            Assert.IsTrue(dynSettings.Controller.CustomNodeLoader.AddFileToPath(Path.Combine(_defsPath, "ConnectPoints.dyf")));

            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);
        }

        [Test]
        public void ScalableGraphFunctionSample()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_samplesPath, @".\05 Graph Function\scalable graph function.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Assert.IsTrue(dynSettings.Controller.CustomNodeLoader.AddFileToPath(Path.Combine(_defsPath, "Cf(dx).dyf")));

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void GraphFunctionSample()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_samplesPath, @".\05 Graph Function\graph function.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Assert.IsTrue(dynSettings.Controller.CustomNodeLoader.AddFileToPath(Path.Combine(_defsPath, "GraphFunction.dyf")));

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        #region Python samples
        /*
        [Test]
        public void ConnectTwoPointArraysWithoutPython()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\connect two point arrays without python.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void ConnectTwoPointArrays()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\connect two point arrays.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        
        [Test]
        public void CreateSineWaveFromSelectedCurve()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\create sine wave from selected curve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            CurveByPoints cbp = null;
            using (_trans = new Transaction(dynRevitSettings.Doc.Document))
            {
                _trans.Start("Create reference points for testing Python node.");

                ReferencePoint p1 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ());
                ReferencePoint p2 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ(0,10,0));
                ReferencePoint p3 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ(0,20,0));
                ReferencePointArray ptArr = new ReferencePointArray();
                ptArr.Append(p1);
                ptArr.Append(p2);
                ptArr.Append(p3);

                cbp = dynRevitSettings.Doc.Document.FamilyCreate.NewCurveByPoints(ptArr);

                _trans.Commit();
            }

            Assert.IsNotNull(cbp);

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var selectionNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynCurvesBySelection).First();
            ((dynCurvesBySelection)selectionNode).SelectedElement = cbp;

            //delete the transaction node when testing
            //var transNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynTransaction).First();
            //dynRevitSettings.Controller.RunCommand(vm.DeleteCommand, transNode);

            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }

        [Test]
        public void CreateSineWaveFromSelectedPoints()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_samplesPath, @".\06 Python Node\create sine wave from selected points.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ReferencePoint p1 = null;
            ReferencePoint p2 = null;

            using (_trans = new Transaction(dynRevitSettings.Doc.Document))
            {
                _trans.Start("Create reference points for testing python node.");

                p1 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ());
                p2 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ(0, 10, 0));

                _trans.Commit();
            }

            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            var selectionNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynPointBySelection);
            Assert.AreEqual(2, selectionNodes.Count());

            ((dynPointBySelection)selectionNodes.ElementAt(0)).SelectedElement = p1;
            ((dynPointBySelection)selectionNodes.ElementAt(1)).SelectedElement = p2;

            //delete the transaction node when testing
            //var transNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynTransaction).First();
            //dynRevitSettings.Controller.RunCommand(vm.DeleteCommand, transNode);

            dynSettings.Controller.DynamoViewModel.RunExpressionCommand.Execute(true);
        }
        */
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
            Assert.AreEqual(typeSelNode.SelectedIndex, count - 1);

            //now try and set the selected index to something
            //greater than what is possible
            typeSelNode.SelectedIndex = count + 5;
            Assert.AreEqual(typeSelNode.SelectedIndex, -1);
        }

        [Test]
        public void ModelCurveNode()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string samplePath = Path.Combine(_testPath, @".\ModelCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);
            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);

            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(CurveElement));

            //verify one model curve created
            int count = fec.ToElements().Count;
            Assert.IsInstanceOf(typeof(ModelCurve), fec.ToElements().First());
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
        public void SwitchDocuments()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            //open the workflow and run the expression
            string testPath = Path.Combine(_testPath, "ReferencePointTest.dyn");
            dynSettings.Controller.RunCommand(vm.OpenCommand,testPath);
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
        public void CurveByPoints()
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
        public void XYZFromReferencePoint()
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
        public void CurveByPointsByLine()
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
            var node = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is dynDoubleInput).First();
            ((dynBasicInteractive<double>)node).Value = 12.0;

            dynSettings.Controller.RunCommand(vm.RunExpressionCommand, true);
            fec = null;
            fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(2, fec.ToElements().Count);
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
        public void AdaptiveComponents()
        {
            DynamoViewModel vm = dynSettings.Controller.DynamoViewModel;

            string path = Path.Combine(_testPath, @".\AdaptiveComponents\AdaptiveComponentSample.rfa");
            string modelPath = Path.GetFullPath(path);
            SwapCurrentModel(modelPath);

            string samplePath = Path.Combine(_testPath, @".\AdaptiveComponents\AdaptiveComponents.dyn");
            string testPath = Path.GetFullPath(samplePath);

            dynSettings.Controller.RunCommand(vm.OpenCommand, testPath);

            //the .dyn has the slider set at 5. let's make sure that
            //if you set the slider to somethin else before running, that it get the correct number
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

            //reset the original model
            SwapCurrentModel(_emptyModelPath);
        }
        
        /// <summary>
        /// Opens and activates a new model, and closes the old model.
        /// </summary>
        private static void SwapCurrentModel(string modelPath)
        {
            Document initialDoc = dynRevitSettings.Doc.Document;
            dynRevitSettings.Revit.OpenAndActivateDocument(modelPath);
            initialDoc.Close(false);
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
