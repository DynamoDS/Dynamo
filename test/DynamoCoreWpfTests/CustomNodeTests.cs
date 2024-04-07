using System;
using System.IO;
using System.Linq;
using CoreNodeModels;
using CoreNodeModels.Input;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Tests;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    class CustomNodeTests :  DynamoViewModelUnitTest
    {
        [Test]
        public void TestNewNodeFromSelectionCommandState()
        {
            Assert.IsFalse(ViewModel.CurrentSpaceViewModel.NodeFromSelectionCommand.CanExecute(null));

            var node = new DoubleInput();
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(node, false);
            ViewModel.Model.AddToSelection(node);
            Assert.IsTrue(ViewModel.CurrentSpaceViewModel.NodeFromSelectionCommand.CanExecute(null));

            var ws = ViewModel.Model.CustomNodeManager.Collapse(
                DynamoSelection.Instance.Selection.OfType<NodeModel>(),
                Enumerable.Empty<NoteModel>(),
                ViewModel.Model.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest2__",
                    Success = true
                });

            Assert.IsFalse(ViewModel.CurrentSpaceViewModel.NodeFromSelectionCommand.CanExecute(null));
        }

        [Test]
        public void TestNestedGroupsOnCustomNodes()
        {
            string openPath = Path.Combine(TestDirectory, @"core\collapse\collapse-group.dyn");
            OpenModel(openPath);

            foreach (var node in ViewModel.CurrentSpaceViewModel.Nodes)
            {
                DynamoSelection.Instance.Selection.Add(node.NodeModel);
            }
            foreach (var note in ViewModel.CurrentSpaceViewModel.Notes)
            {
                DynamoSelection.Instance.Selection.Add(note.Model);
            }
            foreach (var grp in ViewModel.CurrentSpaceViewModel.Annotations)
            {
                DynamoSelection.Instance.Selection.Add(grp.AnnotationModel);
            }

            //assert that everything is selected
            var groupsSelected = DynamoSelection.Instance.Selection.OfType<AnnotationModel>().Count();
            var nodesSelected = DynamoSelection.Instance.Selection.OfType<NodeModel>().Count();
            var notesSelected = DynamoSelection.Instance.Selection.OfType<NoteModel>().Count();
            Assert.AreEqual(ViewModel.CurrentSpaceViewModel.Annotations.Count(), groupsSelected); // 3 groups
            Assert.AreEqual(ViewModel.CurrentSpaceViewModel.Nodes.Count(), nodesSelected); //6 nodes
            Assert.AreEqual(ViewModel.CurrentSpaceViewModel.Notes.Count(), notesSelected); // 2 notes

            var ws = ViewModel.Model.CustomNodeManager.Collapse(
                DynamoSelection.Instance.Selection.OfType<NodeModel>(),
                DynamoSelection.Instance.Selection.OfType<NoteModel>(),
                ViewModel.Model.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest3__",
                    Success = true
                });

            ViewModel.Model.AddCustomNodeWorkspace(ws);
            ViewModel.Model.OpenCustomNodeWorkspace(ws.CustomNodeId);
            var customNodeWorkspace = ViewModel.Model.CurrentWorkspace;

            Assert.AreEqual(groupsSelected, customNodeWorkspace.Annotations.Count());
            Assert.AreEqual(nodesSelected, customNodeWorkspace.Nodes.Count() - 1); //Subtract 1 for the Output Node
            Assert.AreEqual(notesSelected, customNodeWorkspace.Notes.Count());

            //assert that note and groups are still associated
            var maingroup = customNodeWorkspace.Annotations.First(x => x.AnnotationText.Equals("Test"));
            Assert.AreEqual(2, maingroup.Nodes.OfType<AnnotationModel>().Count()); // 2 groups inside the main group
            Assert.AreEqual(1, maingroup.Nodes.OfType<NoteModel>().Count()); // 1 note inside the main group
        }

        [Test]
        public void TestDataInputInitializationTest()
        {
            var node = new DefineData();
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(node, false);

            var supportedDynamoTypesList = DSCore.Data.DataNodeDynamoTypeList;

            // Assert - default node values
            Assert.AreEqual(node.SelectedString, supportedDynamoTypesList.First().Name);
            Assert.AreEqual(node.DisplayValue, CoreNodeModels.Properties.Resources.DefineDataDisplayValueMessage);
            Assert.AreEqual(node.PlayerValue, string.Empty);
            Assert.IsFalse(node.IsAutoMode);
            Assert.IsFalse(node.IsList);
        }

        [Test]
        public void TestDefineDataAutoModeType()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\defineData\defineDataTest.dyn");
            OpenModel(path);

            var lockedGUID = "e02596f9-9e1f-43a2-9f61-9909ec58ca34";
            var unlockedGUID = "ee557143-64bc-4961-981c-2794af48b79f";

            var lockedNode = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace<DefineData>(
                Guid.Parse(lockedGUID));

            var unlockedNode = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace<DefineData>(
                Guid.Parse(unlockedGUID));

            RunCurrentModel();

            var nodes = ViewModel.CurrentSpaceViewModel.Nodes;

            Assert.IsFalse(lockedNode.IsAutoMode);
            Assert.IsTrue(lockedNode.DisplayValue == "Boolean");
            Assert.IsTrue(lockedNode.Infos.Count == 1);

            Assert.IsTrue(unlockedNode.IsAutoMode);
            Assert.IsTrue(unlockedNode.DisplayValue == "Integer");

            // Why is this still showing an internal error?
            //Assert.IsTrue(unlockedNode.Infos.Count == 0, "The AutoMode node should have found the correct type and have no Errors, but it does .. ");
        }

        [Test]
        public void TestDefineDataCorrectInheritanceDisplayedInAutoMode()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\defineData\defineDataTest.dyn");
            OpenModel(path);

            var listGUID = "0cbb9f47-5a28-4898-8d28-575cb15c4455";  // List inputs are 'Line' and 'Rectangle', we expect a 'Curve' as DisplayValue
            var errorListGUID = "39edfbf6-a83b-4815-9bb0-2d7ebcff39f3"; // List inputs are 'Line', 'Rectangle' and 5 (integer), we expect a 'Select Types' as DisplayValue

            var listNode = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace<DefineData>(
                Guid.Parse(listGUID));

            var errorListNode = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace<DefineData>(
             Guid.Parse(errorListGUID));

            RunCurrentModel();

            var nodes = ViewModel.CurrentSpaceViewModel.Nodes;

            Assert.IsTrue(listNode.IsAutoMode);
            Assert.IsTrue(listNode.IsList);
            Assert.AreEqual(listNode.DisplayValue, "Curve", "The correct common ancestor should be Curve, not 'Line' or 'Rectangle'");

            Assert.IsTrue(errorListNode.IsAutoMode);
            Assert.IsFalse(errorListNode.IsList); 
            Assert.AreEqual(errorListNode.DisplayValue, "Select Types", "The node displays a type value, but it should not.");
        }

    }
}
