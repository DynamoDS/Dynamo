using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitServices.Persistence;

namespace Dynamo.Tests
{
    [TestFixture]
    class SelectionTests: DynamoRevitUnitTestBase
    {
        [Test]
        public void FamilyTypeSelectorNode()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Selection\SelectFamily.dyn");
            string testPath = Path.GetFullPath(samplePath);

            //open the test file
            model.Open(testPath);

            //first assert that we have only one node
            var nodeCount = dynSettings.Controller.DynamoModel.Nodes.Count;
            Assert.AreEqual(1, nodeCount);

            //assert that we have the right number of family symbols
            //in the node's items source
            FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.GetInstance().CurrentUIDocument.Document);
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
        public void AllSelectionNodes()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Selection\Selection.dyn");
            string testPath = Path.GetFullPath(samplePath);

            //open the test file
            model.Open(testPath);

            Assert.DoesNotThrow(()=>dynSettings.Controller.RunExpression(true));

            var selNodes = model.AllNodes.Where(x => x is SelectionBase || x is MultipleElementSelectionBase);
            Assert.IsFalse(selNodes.Any(x=>x.OldValue == null));
        }
    }
}
