using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class FamilyTests:DynamoRevitUnitTestBase
    {
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
        public void GetFamilyInstancesByType()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\GetFamilyInstancesByType.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            var node =
                (GetFamilyInstancesByType)dynSettings.Controller.DynamoModel.Nodes.First(x => x is GetFamilyInstancesByType);
            Assert.IsTrue(node.OldValue.IsList);

            var list = ((FScheme.Value.List)node.OldValue).Item;
            Assert.AreEqual(100, list.Count());
        }

        [Test]
        public void GetFamilyInstanceLocation()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\GetFamilyInstanceLocation.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }
    }
}
