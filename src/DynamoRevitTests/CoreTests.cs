using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitServices.Persistence;

namespace Dynamo.Tests
{
    [TestFixture]
    class CoreTests:DynamoRevitUnitTestBase
    {
        /// <summary>
        /// Sanity Check graph should always throw an error.
        /// </summary>
        [Test]
        public void SanityCheck()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Core\SanityCheck.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.Throws(typeof(AssertionException), () => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void CanChangeLacingAndHaveElementsUpdate()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Core\LacingTest.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            var xyzNode = dynSettings.Controller.DynamoModel.Nodes.First(x => x is Xyz);
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

        /*

        [Test]
        public void ElementNodeReassociation()
        {
            var model = dynSettings.Controller.DynamoModel;

            string testPath = Path.Combine(_testPath, @".\ReferencePoint\ReferencePoint.dyn");
            model.Open(testPath);
            Assert.AreEqual(3, dynSettings.Controller.DynamoModel.Nodes.Count);

            var refPtNode = model.CurrentWorkspace.FirstNodeFromWorkspace<ReferencePointByXyz>();
            refPtNode.ArgumentLacing = LacingStrategy.Longest;

            dynSettings.Controller.RunExpression();

            var oldVal = (ReferencePoint)((FScheme.Value.Container)refPtNode.OldValue).Item;
            refPtNode.ResetOldValue();

            var oldId = oldVal.Id;
            var oldPos = oldVal.Position;

            model.CurrentWorkspace.FirstNodeFromWorkspace<DoubleInput>().Value = "1";

            dynSettings.Controller.RunExpression();

            var newVal = (ReferencePoint)((FScheme.Value.Container)refPtNode.OldValue).Item;

            Assert.AreEqual(oldId, newVal.Id);
            Assert.AreNotEqual(oldPos, newVal.Position);

            refPtNode.ResetOldValue();

            var numberNode = model.CurrentWorkspace.FirstNodeFromWorkspace<DoubleInput>();
            numberNode.Value = "0..10";

            dynSettings.Controller.RunExpression();

            var multipleValues = ((FScheme.Value.List)refPtNode.OldValue).Item;
            Assert.AreEqual(11, multipleValues.Length);
            Assert.IsTrue(multipleValues.Any(x => ((ReferencePoint)((FScheme.Value.Container)x).Item).Id == oldId));

            refPtNode.ResetOldValue();

            numberNode.Value = "0";

            dynSettings.Controller.RunExpression();

            var finalVal = (ReferencePoint)((FScheme.Value.Container)refPtNode.OldValue).Item;
            Assert.AreEqual(oldId, finalVal.Id);
        }
        */
        [Test]
        public void SwitchDocuments()
        {
            var model = dynSettings.Controller.DynamoModel;

            //open the workflow and run the expression
            string testPath = Path.Combine(_testPath, @".\ReferencePoint\ReferencePoint.dyn");
            model.Open(testPath);
            Assert.AreEqual(3, dynSettings.Controller.DynamoModel.Nodes.Count());
            Assert.DoesNotThrow(()=>dynSettings.Controller.RunExpression(true));

            //verify we have a reference point
            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(1, fec.ToElements().Count());

            //open a new document and activate it
            UIDocument initialDoc = DocumentManager.GetInstance().CurrentUIApplication.ActiveUIDocument;
            string shellPath = Path.Combine(_testPath, @".\empty1.rfa");
            DocumentManager.GetInstance().CurrentUIApplication.OpenAndActivateDocument(shellPath);
            initialDoc.Document.Close(false);

            ////assert that the doc is set on the controller
            Assert.IsNotNull(dynRevitSettings.Doc.Document);

            ////update the double node so the graph reevaluates
            var doubleNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is BasicInteractive<double>);
            BasicInteractive<double> node = doubleNodes.First() as BasicInteractive<double>;
            node.Value = node.Value + .1;

            ////run the expression again
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
            //fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            //fec.OfClass(typeof(ReferencePoint));
            //Assert.AreEqual(1, fec.ToElements().Count());

            //finish out by restoring the original
            //initialDoc = DocumentManager.GetInstance().CurrentUIApplication.ActiveUIDocument;
            //shellPath = Path.Combine(_testPath, @"empty.rfa");
            //DocumentManager.GetInstance().CurrentUIApplication.OpenAndActivateDocument(shellPath);
            //initialDoc.Document.Close(false);

        }

        [Test, TestCaseSource("SetupCopyPastes")]
        public void CanCopyAndPasteAllNodesOnRevit(string typeName)
        {
            var model = dynSettings.Controller.DynamoModel;

            Assert.DoesNotThrow(() => model.CreateNode(0, 0, typeName), string.Format("Could not create node : {0}", typeName));

                var node = model.AllNodes.FirstOrDefault();

                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.Add(node);
                Assert.AreEqual(1, DynamoSelection.Instance.Selection.Count);

                Assert.DoesNotThrow(() => model.Copy(null), string.Format("Could not copy node : {0}", node.GetType()));
                Assert.DoesNotThrow(() => model.Paste(null), string.Format("Could not paste node : {0}", node.GetType()));

                model.Clear(null);    
        }

        static List<string> SetupCopyPastes()
        {
            var excludes = new List<string>();
            excludes.Add("Dynamo.Nodes.DSFunction");
            excludes.Add("Dynamo.Nodes.Symbol");
            excludes.Add("Dynamo.Nodes.Output");
            excludes.Add("Dynamo.Nodes.Function");
            excludes.Add("Dynamo.Nodes.LacerBase");
            excludes.Add("Dynamo.Nodes.FunctionWithRevit");
            return dynSettings.Controller.BuiltInTypesByName.Where(x => !excludes.Contains(x.Key)).Select(kvp => kvp.Key).ToList();
        }
    }
}
