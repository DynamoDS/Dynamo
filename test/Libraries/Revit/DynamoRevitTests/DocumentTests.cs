using System.IO;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Dynamo.Nodes;

using NUnit.Framework;
using RevitServices.Persistence;
using RevitServices.Transactions;

using RTF.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class DocumentTests : DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@"./empty.rfa")]
        public void InitialUIDocumentIsNotNull()
        {
            Assert.IsNotNull(DocumentManager.Instance.CurrentUIDocument);
        }

        [Test]
        [TestModel(@"./empty.rfa")]
        public void OpeningNewDocumentDoesNotSwitchUIDocument()
        {
            // a reference to the initial document
            var initialDoc = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;

            var newDoc = OpenAndActivateNewModel(_emptyModelPath1);

            // Assert that the active UI document is
            // still the initial document
            Assert.AreEqual(DocumentManager.Instance.CurrentUIDocument.Document.PathName, initialDoc.Document.PathName);
        }

        [Test]
        [TestModel(@"./empty.rfa")]
        public void SwitchingViewAwayFromCachedDocumentDisablesRun()
        {
            // empty.rfa will be open at test start
            // swap documents and ensure that 
            var initialDoc = DocumentManager.Instance.CurrentUIDocument;
            var newDoc = OpenAndActivateNewModel(_emptyModelPath1);

            Assert.False(ViewModel.RunEnabled);
        }

        [Test]
        [TestModel(@"./empty.rfa")]
        public void RunIsDisabledWhenOpeningADocumentinPerspectiveView()
        {
            // Swap to a document that only has one open perspective view
            SwapCurrentModel(Path.Combine(_testPath, "model_with_box.rvt"));

            Assert.False(ViewModel.RunEnabled);

            // Then you need to swap back because the journal's ID_FLUSH_UNDO
            // is disabled in perspective as well

            SwapCurrentModel(_emptyModelPath);
        }

        [Test, Ignore]
        [TestModel(@"./empty.rfa")]
        public void AttachesToNewDocumentWhenAllDocsWereClosed()
        {
            Assert.Inconclusive("Cannot test. API required for allowing closing all docs.");
        }

        [Test, Ignore]
        [TestModel(@"./empty.rfa")]
        public void WhenActiveDocumentResetIsRequiredVisualizationsAreCleared()
        {
            Assert.Inconclusive("Cannot test. API required for allowing closing all docs.");
        }

        // Test moved from old CoreTests class.
        [Test, Category("Failure")]
        [TestModel(@".\empty.rfa")]
        public void SwitchDocuments()
        {
            //open the workflow and run the expression
            string testPath = Path.Combine(_testPath, @".\ReferencePoint\ReferencePoint.dyn");
            ViewModel.OpenCommand.Execute(testPath);
            Assert.AreEqual(3, ViewModel.Model.Nodes.Count());
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            //verify we have a reference point
            var fec = new FilteredElementCollector((Autodesk.Revit.DB.Document)DocumentManager.Instance.CurrentDBDocument);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(1, fec.ToElements().Count());

            //open a new document and activate it
            var initialDoc = (UIDocument)DocumentManager.Instance.CurrentUIDocument;
            string shellPath = Path.Combine(_testPath, @".\empty1.rfa");
            TransactionManager.Instance.ForceCloseTransaction();
            ((UIApplication)DocumentManager.Instance.CurrentUIApplication).OpenAndActivateDocument(shellPath);
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
            //shellPath = Path.Combine(_testPath, @"empty.rfa");
            //DocumentManager.GetInstance().CurrentUIApplication.OpenAndActivateDocument(shellPath);
            //initialDoc.Document.Close(false);

        }
    }
}
