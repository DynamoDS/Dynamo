using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    internal class DummyNodeTests : DynamoModelTestBase
    {
        string testFileWithDummyNode = @"core\dummy_node\2080_JSONTESTCRASH undo_redo.dyn";
        string testFileWithXmlDummyNode = @"core\dummy_node\dummyNode.dyn";
        string testFileWithMultipleXmlDummyNode = @"core\dummy_node\dummyNodeXMLMultiple.dyn";


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

        [Test]
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

        [Test]
        public void WorkspaceIsReadonly_IfXmlDummyNodePresent()
        {
            string openPath = Path.Combine(TestDirectory, testFileWithXmlDummyNode);
            OpenModel(openPath);

            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DummyNode>().Count());
            var dummyNode = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DummyNode>().First();
            Assert.IsTrue(this.CurrentDynamoModel.CurrentWorkspace.IsReadOnly);

            //delete the dummy node
            this.CurrentDynamoModel.CurrentWorkspace.RemoveAndDisposeNode(dummyNode);
            Assert.IsFalse(this.CurrentDynamoModel.CurrentWorkspace.IsReadOnly);
        }

        [Test]
        public void IfDeserializationThrowsDummyNodeStillCreated()
        {
            string testFileWithUknownAssembly = @"core\dummy_node\unknownAssemblyName.dyn";

            var exceptionCount = 0;
            var handler = new ResolveEventHandler((o, e) =>
            {
               
                if (e.Name.Contains("UNKNOWNASSEMBLY"))
                {
                    exceptionCount = exceptionCount + 1;
                    throw new Exception("TestingTesting");
                }
                return null;
            });
            //attach our handler that will throw
            AppDomain.CurrentDomain.AssemblyResolve += handler;

            string openPath = Path.Combine(TestDirectory, testFileWithUknownAssembly);
            OpenModel(openPath);

            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DummyNode>().Count());
            var dummyNode = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DummyNode>().First();
            //runtime behavior has changed for assembly resolve handlers that throw exceptions.
#if NET6_0_OR_GREATER
            Assert.AreEqual(2, exceptionCount);
#elif NETFRAMEWORK
            Assert.AreEqual(1, exceptionCount);
#endif
            AppDomain.CurrentDomain.AssemblyResolve -= handler;
        }

        [Test]
        public void ResolveDummyNodesOnDownloadingPackage()
        {
            string path = Path.Combine(TestDirectory, @"core\packageDependencyTests\ResolveDummyNodesOnDownloadingPackage.dyn");
            OpenModel(path);

            var dummyNodes = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DummyNode>();
            Assert.AreEqual(2, dummyNodes.Count());

            var output = GetPreviewValue("a1aba50a873443f2bfb88480a89e3f36");
            Assert.IsNull(output);

            // Load the Dynamo Samples package and verify that the dummy nodes have been resolved.
            string packageDirectory = Path.Combine(TestDirectory, @"pkgs\Dynamo Samples");
            LoadPackage(packageDirectory);

            dummyNodes = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DummyNode>();
            Assert.AreEqual(0, dummyNodes.Count());
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.HasUnsavedChanges, false);

            output = GetPreviewValue("a1aba50a873443f2bfb88480a89e3f36");
            Assert.AreEqual(42, output);
        }

        [Test]
        public void ResolveDummyNodesInsideCustomNodeWorkspace()
        {
            // Validating the case when dummy nodes are resolved inside a custom node workspace.
            String path = Path.Combine(TestDirectory, @"core\packageDependencyTests\ResolveDummyNodesInsideCustomNodeWorkspace.dyf");
            OpenModel(path);

            Assert.IsInstanceOf<CustomNodeWorkspaceModel>(CurrentDynamoModel.CurrentWorkspace);

            var dummyNodes = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DummyNode>();
            Assert.AreEqual(2, dummyNodes.Count());

            // Load the Dynamo Samples package and verify that the dummy nodes have been resolved.
            var packageDirectory = Path.Combine(TestDirectory, @"pkgs\Dynamo Samples");
            LoadPackage(packageDirectory);

            dummyNodes = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DummyNode>();
            Assert.AreEqual(0, dummyNodes.Count());
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.HasUnsavedChanges, false);
        }

        [Test]
        public void DummyNodesWarningMessageTest()
        {
            String path = Path.Combine(TestDirectory, @"core\dummy_node\DummyNodesWarningMessageTest.dyn");
            OpenModel(path);

            var dummyNodes = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DummyNode>();
            Assert.AreEqual(2, dummyNodes.Count());

            // Asserting the warning message that is displayed for zerotouch and nodemodel dummy nodes.
            var zeroTouchDummyNode = dummyNodes.First();
            Assert.AreEqual(zeroTouchDummyNode.NodeNature, DummyNode.Nature.Unresolved);
            Assert.AreEqual(zeroTouchDummyNode.FunctionName, "HowickMaker.hMember.ByLineVector");
            Assert.AreEqual(zeroTouchDummyNode.GetDescription(), "Node 'HowickMaker.hMember.ByLineVector' cannot be resolved.");

            var nodeModelDummyNode = dummyNodes.Last();
            Assert.AreEqual(nodeModelDummyNode.NodeNature, DummyNode.Nature.Unresolved);
            Assert.AreEqual(nodeModelDummyNode.LegacyAssembly, "SampleLibraryUI");
            Assert.AreEqual(nodeModelDummyNode.GetDescription(), "Node of type 'SampleLibraryUI.Examples.DropDownExample', from assembly 'SampleLibraryUI', cannot be resolved.");
        }
    }
}
