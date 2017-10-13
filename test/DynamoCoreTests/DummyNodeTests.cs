using Dynamo.Graph.Nodes;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Models;

namespace Dynamo.Tests
{
    internal class DummyNodeTests : DynamoModelTestBase
    {
        string testFileWithDummyNode = @"core\dummy_node\2080_JSONTESTCRASH undo_redo.dyn";

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void CanCopyPasteJSONDummyNodeAndRetainsOriginalJSON()
        {
            string openPath = Path.Combine(TestDirectory, testFileWithDummyNode);
            OpenModel(openPath);

            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DummyNode>().Count());
            //select the dummy node
            CurrentDynamoModel.AddToSelection(CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DummyNode>().First());
            CurrentDynamoModel.Copy();
            CurrentDynamoModel.Paste();
            //get both dummyNodes
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DummyNode>().Count());
            var dummies = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DummyNode>().ToList();

            var oc1 = (dummies[0].OriginalNodeContent as JObject).ToString();
            var oc2 = (dummies[1].OriginalNodeContent as JObject).ToString();

            Console.WriteLine(oc1);
            Console.WriteLine(oc2);
            //assert that originalData are the same string
            Assert.AreEqual(oc1, oc2);

        }

        [Test, Category("Failure")]
        public void CanMoveAndUndoJSONDummyNodeAndRetainsOriginalJSON()
        {
            string openPath = Path.Combine(TestDirectory, testFileWithDummyNode);
            OpenModel(openPath);

            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DummyNode>().Count());
            var dummyNode = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DummyNode>().First();
            var originalPos = dummyNode.X;
            var contentPreMove = (dummyNode.OriginalNodeContent as JObject).ToString();
            //move the node.
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(dummyNode.GUID, "Position", "12.0;34.1"));
            //assert we moved.
            Assert.AreEqual(12, dummyNode.X);
            //assert same content after move.
            Assert.AreEqual(contentPreMove, (dummyNode.OriginalNodeContent as JObject).ToString());

            //undo the move.
            CurrentDynamoModel.CurrentWorkspace.Undo();
            Assert.AreEqual(originalPos, dummyNode.X);

            //assert the content is the same as when we started.
            Assert.AreEqual(contentPreMove, (dummyNode.OriginalNodeContent as JObject).ToString());
        }
    }
}