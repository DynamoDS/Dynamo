using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;

using NUnit.Framework;

using RevitServices.Persistence;
using RevitServices.Transactions;

using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    class CoreTests : SystemTest
    {
        /// <summary>
        /// Sanity Check graph should always have nodes that error.
        /// </summary>
        [Test]
        [TestModel(@".\empty.rfa")]
        public void SanityCheck()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\Core\SanityCheck.dyn");
            string testPath = Path.GetFullPath(samplePath);

            //Assert that there are some errors in the graph
            ViewModel.OpenCommand.Execute(testPath);
            ViewModel.Model.RunExpression();
            var errorNodes = model.Nodes.Where(x => x.State == ElementState.Warning);
            Assert.Greater(errorNodes.Count(), 0);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanChangeLacingAndHaveElementsUpdate()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Core\LacingTest.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            var xyzNode = ViewModel.Model.Nodes.First(x => x.NickName == "Point.ByCoordinates");
            Assert.IsNotNull(xyzNode);
            
            //test the shortest lacing
            xyzNode.ArgumentLacing = LacingStrategy.Shortest;

            ViewModel.Model.RunExpression();

            var fec = new FilteredElementCollector((Autodesk.Revit.DB.Document)DocumentManager.Instance.CurrentDBDocument);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(4, fec.ToElements().Count());

            //test the longest lacing
            xyzNode.ArgumentLacing = LacingStrategy.Longest;
            ViewModel.Model.RunExpression();
            fec = null;

            fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);

            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(5, fec.ToElements().Count());

            //test the cross product lacing
            xyzNode.ArgumentLacing = LacingStrategy.CrossProduct;
            ViewModel.Model.RunExpression();
            fec = null;

            fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);

            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(20, fec.ToElements().Count());
        }

        [Test, Category("Failure")]
        [TestModel(@".\empty.rfa")]
        public void SwitchDocuments()
        {
            //open the workflow and run the expression
            string testPath = Path.Combine(workingDirectory, @".\ReferencePoint\ReferencePoint.dyn");
            ViewModel.OpenCommand.Execute(testPath);
            Assert.AreEqual(3, ViewModel.Model.Nodes.Count());
            Assert.DoesNotThrow(()=>ViewModel.Model.RunExpression());

            //verify we have a reference point
            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);

            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(1, fec.ToElements().Count());

            //open a new document and activate it
            var initialDoc = (UIDocument)DocumentManager.Instance.CurrentUIDocument;
            string shellPath = Path.Combine(workingDirectory, @".\empty1.rfa");
            TransactionManager.Instance.ForceCloseTransaction();
            DocumentManager.Instance.CurrentUIApplication.OpenAndActivateDocument(shellPath);

            initialDoc.Document.Close(false);

            ////assert that the doc is set on the DocumentManager
            Assert.IsNotNull((Document)DocumentManager.Instance.CurrentDBDocument);

            ////update the double node so the graph reevaluates
            var doubleNodes = ViewModel.Model.Nodes.Where(x => x is BasicInteractive<double>);
            BasicInteractive<double> node = doubleNodes.First() as BasicInteractive<double>;
            node.Value = node.Value + .1;

            ////run the expression again
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
            //fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            //fec.OfClass(typeof(ReferencePoint));
            //Assert.AreEqual(1, fec.ToElements().Count());

            //finish out by restoring the original
            //initialDoc = DocumentManager.GetInstance().CurrentUIApplication.ActiveUIDocument;
            //shellPath = Path.Combine(workingDirectory, @"empty.rfa");
            //DocumentManager.GetInstance().CurrentUIApplication.OpenAndActivateDocument(shellPath);
            //initialDoc.Document.Close(false);

        }

        [Test, TestCaseSource("SetupCopyPastes")]
        [TestModel(@".\empty.rfa")]
        public void CanCopyAndPasteAllNodesOnRevit(string typeName)
        {
            var model = ViewModel.Model;

            Assert.DoesNotThrow(() => model.CurrentWorkspace.AddNode(0, 0, typeName), string.Format("Could not create node : {0}", typeName));

            var node = model.AllNodes.FirstOrDefault();

            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(node);
            Assert.AreEqual(1, DynamoSelection.Instance.Selection.Count);

            Assert.DoesNotThrow(() => model.Copy(null), string.Format("Could not copy node : {0}", node.GetType()));
            Assert.DoesNotThrow(() => model.Paste(null), string.Format("Could not paste node : {0}", node.GetType()));

            model.Clear(null);
        }

        private List<string> SetupCopyPastes()
        {
            var excludes = new List<string>();
            excludes.Add("Dynamo.Nodes.DSFunction");
            excludes.Add("Dynamo.Nodes.Symbol");
            excludes.Add("Dynamo.Nodes.Output");
            excludes.Add("Dynamo.Nodes.Function");
            excludes.Add("Dynamo.Nodes.LacerBase");
            excludes.Add("Dynamo.Nodes.FunctionWithRevit");
            return ViewModel.Model.BuiltInTypesByName.Where(x => !excludes.Contains(x.Key)).Select(kvp => kvp.Key).ToList();
        }
    }
}
