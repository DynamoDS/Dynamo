using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using DSRevitNodesUI;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitServices.Persistence;
using RTF.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class SelectionTests: DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void FamilyTypeSelectorNode()
        {
            string samplePath = Path.Combine(_testPath, @".\Selection\SelectFamily.dyn");
            string testPath = Path.GetFullPath(samplePath);

            //open the test file
            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            //first assert that we have only one node
            var nodeCount = dynSettings.Controller.DynamoModel.Nodes.Count;
            Assert.AreEqual(1, nodeCount);

            //assert that we have the right number of family symbols
            //in the node's items source
            FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
            fec.OfClass(typeof(Family));
            int count = 0;
            foreach (Family f in fec.ToElements())
            {
                foreach (FamilySymbol fs in f.Symbols)
                {
                    count++;
                }
            }

            FamilyTypes typeSelNode = (FamilyTypes)dynSettings.Controller.DynamoModel.Nodes.First();
            Assert.AreEqual(typeSelNode.Items.Count, count);

            //assert that the selected index is correct
            Assert.AreEqual(typeSelNode.SelectedIndex, 3);

            //now try and set the selected index to something
            //greater than what is possible
            typeSelNode.SelectedIndex = count + 5;
            Assert.AreEqual(typeSelNode.SelectedIndex, -1);

            //Assert.Inconclusive("Porting : FamilyTypeSelector");
        }

        [Test]
        [TestModel(@".\Selection\Selection.rfa")]
        public void AllSelectionNodes()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Selection\Selection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            //open the test file
            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

            var selNodes = model.AllNodes.Where(x => x is DSElementSelection || x is DSModelElementsSelection);
            Assert.IsFalse(selNodes.Any(x => x.CachedValue == null));

            //Assert.Inconclusive("Porting : MultipleElementSelectionBase");
        }
    }
}
