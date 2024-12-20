using System;
using System.Linq;
using System.Windows.Input;
using Dynamo.Selection;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    public class NoteViewTests : DynamoTestUIBase
    {
        // adapted from NodeViewTests.cs

        public override void Open(string path)
        {
            base.Open(path);

            DispatcherUtil.DoEvents();
        }

        public override void Run()
        {
            base.Run();

            DispatcherUtil.DoEvents();
        }

        [Test]
        public void T01_ZIndex_Test_MouseDown()
        {
            // Reset zindex to start value.
            Dynamo.ViewModels.NoteViewModel.StaticZIndex = 3;
            Open(@"UI\UINotes.dyn");

            var noteView = NoteViewWithGuid("4677e999-d5f5-4bb2-9706-a97bf3a86711");

            // Index of first note == 5.
            Assert.AreEqual(5, noteView.ViewModel.ZIndex);
            Assert.AreEqual(3 + ViewModel.HomeSpace.Notes.Count() + ViewModel.HomeSpace.Nodes.Count(), Dynamo.ViewModels.NoteViewModel.StaticZIndex);

            noteView.noteText.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = Mouse.PreviewMouseDownEvent,
                Source = this,
            });
            Assert.AreEqual(noteView.ViewModel.ZIndex, Dynamo.ViewModels.NoteViewModel.StaticZIndex);
        }

        [Test]
        public void T02_ZIndex_Test_NoteGreaterThanNode()
        {
            //Reset ZIndex to start value for both NoteViewModel and NodeViewModel
            Dynamo.ViewModels.NoteViewModel.StaticZIndex = 3;
            Dynamo.ViewModels.NodeViewModel.StaticZIndex = 3;

            Open(@"UI\UINotes.dyn");
            var noteView = NoteViewWithGuid("4677e999-d5f5-4bb2-9706-a97bf3a86711");

            // Index of First Note (initially) == 5
            Assert.AreEqual(5, noteView.ViewModel.ZIndex);
            Assert.AreEqual(3 + ViewModel.HomeSpace.Notes.Count() + ViewModel.HomeSpace.Nodes.Count(), Dynamo.ViewModels.NoteViewModel.StaticZIndex);

            // Index of First Node (initally) == 4
            var nodeView = NodeViewWithGuid("bbc16882-75c2-4a50-a4e4-5e50e191af8f");
            Assert.AreEqual(4, nodeView.ViewModel.ZIndex);
            Assert.AreEqual(3 + ViewModel.HomeSpace.Nodes.Count(), Dynamo.ViewModels.NodeViewModel.StaticZIndex);


            Assert.Greater(Dynamo.ViewModels.NoteViewModel.StaticZIndex, Dynamo.ViewModels.NodeViewModel.StaticZIndex);
        }

        [Test]
        public void T03_Mousedown_NoteGreaterThanNode()
        {
            //Reset ZIndex to start value for both NoteViewModel and NodeViewModel
            Dynamo.ViewModels.NoteViewModel.StaticZIndex = 3;
            Dynamo.ViewModels.NodeViewModel.StaticZIndex = 3;

            Open(@"UI\UINotes.dyn");
            var noteView = NoteViewWithGuid("4677e999-d5f5-4bb2-9706-a97bf3a86711");
            var nodeView = NodeViewWithGuid("bbc16882-75c2-4a50-a4e4-5e50e191af8f");

            // Click on First Node
            nodeView.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = Mouse.PreviewMouseDownEvent,
                Source = this,
            });
            Assert.AreEqual(5, nodeView.ViewModel.ZIndex);

            // Check if First Note's ZIndex is greater than First Node after click
            Assert.Greater(noteView.ViewModel.ZIndex, nodeView.ViewModel.ZIndex);
        }
        /// <summary>
        /// Check if you can pin node to note
        /// </summary>
        [Test]
        public void CanPinNodeToNode()
        {
            // Open document and get node and note view
            Open(@"UI\UINotes.dyn");
            var nodeGUID = "bbc16882-75c2-4a50-a4e4-5e50e191af8f";
            var noteGUID = "4677e999-d5f5-4bb2-9706-a97bf3a86711";
            var nodeView = NodeViewWithGuid(nodeGUID);
            var noteView = NoteViewWithGuid(noteGUID);

            // Add node to selection and pin note to node
            DynamoSelection.Instance.Selection.AddUnique(nodeView.ViewModel.NodeModel);
            noteView.ViewModel.PinToNodeCommand.Execute(null);

            // Assert that node was pinned
            var pinnedNode = noteView.ViewModel.Model.PinnedNode;
            Assert.IsNotNull(pinnedNode);
            Assert.AreEqual(pinnedNode.GUID.ToString(), nodeGUID);
        }

        /// <summary>
        /// When note is pinned to node,
        /// check that if the note is selected
        /// the pinned node is selected as well
        /// </summary>
        [Test]
        public void CanSelectPinnedNodeThroughNote()
        {
            // Open document and get node and note view
            Open(@"UI\UINotes.dyn");
            var nodeGUID = "bbc16882-75c2-4a50-a4e4-5e50e191af8f";
            var noteGUID = "4677e999-d5f5-4bb2-9706-a97bf3a86711";
            var nodeView = NodeViewWithGuid(nodeGUID);
            var noteView = NoteViewWithGuid(noteGUID);

            // Add node to selection and pin note to node
            DynamoSelection.Instance.Selection.AddUnique(nodeView.ViewModel.NodeModel);
            noteView.ViewModel.PinToNodeCommand.Execute(null);

            // Clear selection
            DynamoSelection.Instance.Selection.Remove(nodeView.ViewModel.NodeModel);

            // Select note
            noteView.noteText.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = Mouse.PreviewMouseDownEvent,
                Source = this,
            });

            // Assert that node and note are selected 
            var nodeIsSelected = DynamoSelection.Instance.Selection.Contains(nodeView.ViewModel.NodeModel);
            var noteIsSelected = DynamoSelection.Instance.Selection.Contains(noteView.ViewModel.Model);
            Assert.IsTrue(nodeIsSelected);
            Assert.IsTrue(noteIsSelected);
        }

        /// <summary>
        /// When note is pinned to node,
        /// check that the note is positioned 
        /// 12 pixels above node 
        /// </summary>
        [Test]
        public void NoteIsPlacedAbovePinnedNode()
        {
            // Open document and get node and note view
            Open(@"UI\UINotes.dyn");
            var nodeGUID = "bbc16882-75c2-4a50-a4e4-5e50e191af8f";
            var noteGUID = "4677e999-d5f5-4bb2-9706-a97bf3a86711";
            var nodeView = NodeViewWithGuid(nodeGUID);
            var noteView = NoteViewWithGuid(noteGUID);

            // Add node to selection and pin note to node
            DynamoSelection.Instance.Selection.AddUnique(nodeView.ViewModel.NodeModel);
            noteView.ViewModel.PinToNodeCommand.Execute(null);

            var distanceToNode = 16;

            var noteModel = noteView.ViewModel.Model;
            var nodeModel = nodeView.ViewModel.NodeModel;

            Assert.AreEqual(noteModel.CenterX, nodeModel.CenterX);
            Assert.AreEqual(noteModel.CenterY, nodeModel.CenterY - (nodeModel.Height * 0.5) - (noteModel.Height * 0.5) - distanceToNode);
        }

        /// <summary>
        /// When note is pinned to node in error state,
        /// check that the note is positioned 
        /// 32 pixels above node
        /// </summary>
        [Test]
        public void NoteIsPlacedAbovePinnedNodeInErrorState()
        {
            // Open document and get node and note view
            Open(@"UI\UINotes.dyn");
            var nodeGUID = "bbc16882-75c2-4a50-a4e4-5e50e191af8f";
            var noteGUID = "4677e999-d5f5-4bb2-9706-a97bf3a86711";
            var nodeView = NodeViewWithGuid(nodeGUID);
            var noteView = NoteViewWithGuid(noteGUID);

            // Get the models to compare positions
            var noteModel = noteView.ViewModel.Model;
            var nodeModel = nodeView.ViewModel.NodeModel;

            // Add error to node to change its state to State.Error
            nodeModel.Error("Sample Error");

            // Add node to selection and pin note to node
            DynamoSelection.Instance.Selection.AddUnique(nodeView.ViewModel.NodeModel);
            noteView.ViewModel.PinToNodeCommand.Execute(null);

            //Assert that distance to node adjusts to error state
            var distanceToNode = 64;
            Assert.AreEqual(noteModel.CenterX, nodeModel.CenterX);
            Assert.AreEqual(noteModel.CenterY, nodeModel.CenterY - (nodeModel.Height * 0.5) - (noteModel.Height * 0.5) - distanceToNode);
        }

        /// <summary>
        /// Makes sure that only when one node and note
        /// are selected the PinToNode command is enabled
        /// </summary>
        [Test]
        public void PinToNodeCommandIsProperlyEnabled()
        {
            // Open document and get nodes and note views
            Open(@"UI\UI_PinNodeToNote.dyn");
            var nodeAGUID = "cf46488c-6631-458e-bba0-25098e2273f5";
            var noteAGUID = "c589c7ff-a481-4a21-bb18-ac01ed9486fc";
            var nodeBGUID = "6af656b8-4762-4e77-a5f9-22ef0ab333b5";
            var noteBGUID = "30858809-6c6b-47a6-a777-4db14c016b0c";
            var nodeAView = NodeViewWithGuid(nodeAGUID);
            var noteAView = NoteViewWithGuid(noteAGUID);
            var nodeBView = NodeViewWithGuid(nodeBGUID);
            var noteBView = NoteViewWithGuid(noteBGUID);

            // Select a node and a note
            DynamoSelection.Instance.Selection.AddUnique(nodeAView.ViewModel.NodeModel);
            DynamoSelection.Instance.Selection.AddUnique(noteAView.ViewModel.Model);

            // Assert that node can be pinned to note
            var canPinANodeToANote = noteAView.ViewModel.PinToNodeCommand.CanExecute(null);
            Assert.IsTrue(canPinANodeToANote);

            //// Assert that you can't pin if you add another node to selection
            DynamoSelection.Instance.Selection.AddUnique(nodeBView.ViewModel.NodeModel);
            var canPinNodesToANote = noteAView.ViewModel.PinToNodeCommand.CanExecute(null);
            Assert.IsFalse(canPinNodesToANote);

            //Remove extra node, Pin and assert that other note cannot be pinned again
            DynamoSelection.Instance.Selection.Remove(nodeBView.ViewModel.NodeModel);
            noteAView.ViewModel.PinToNodeCommand.Execute(null);
            var canPinNodsToNotes = noteBView.ViewModel.PinToNodeCommand.CanExecute(null);
            Assert.IsFalse(canPinNodesToANote);
        }

        /// <summary>
        /// Check that workspace model is aware that changes were made 
        /// when a node is pinned to a note.
        /// </summary>
        [Test]
        public void HasUnsavedChangesWhenPinToNode()
        {
            // Open document and get node and note view
            Open(@"UI\UI_PinNodeToNote.dyn");
            var nodeAGUID = "cf46488c-6631-458e-bba0-25098e2273f5";
            var noteAGUID = "c589c7ff-a481-4a21-bb18-ac01ed9486fc";
            var nodeAView = NodeViewWithGuid(nodeAGUID);
            var noteAView = NoteViewWithGuid(noteAGUID);

            //Assert that workspace doesn't have unsaved changes
            var workspace = nodeAView.ViewModel.WorkspaceViewModel;
            Assert.IsFalse(workspace.HasUnsavedChanges);

            // Select node, note and pin
            DynamoSelection.Instance.Selection.AddUnique(nodeAView.ViewModel.NodeModel);
            DynamoSelection.Instance.Selection.AddUnique(noteAView.ViewModel.Model);
            noteAView.ViewModel.PinToNodeCommand.Execute(null);

            //Assert that workspace has unsaved changes
            Assert.IsTrue(nodeAView.ViewModel.WorkspaceViewModel.HasUnsavedChanges);
        }

        [Test]
        public void UndoPinNode()
        {
            // Open document and get node and note view
            Open(@"UI\UINotes.dyn");
            var nodeGUID = "bbc16882-75c2-4a50-a4e4-5e50e191af8f";
            var noteGUID = "4677e999-d5f5-4bb2-9706-a97bf3a86711";
            var nodeView = NodeViewWithGuid(nodeGUID);
            var noteView = NoteViewWithGuid(noteGUID);

            // Add node to selection and pin note to node
            DynamoSelection.Instance.Selection.AddUnique(nodeView.ViewModel.NodeModel);
            noteView.ViewModel.PinToNodeCommand.Execute(null);

            // Assert that node was pinned
            var pinnedNode = noteView.ViewModel.Model.PinnedNode;
            Assert.IsNotNull(pinnedNode);
            Assert.AreEqual(pinnedNode.GUID.ToString(), nodeGUID);

            //Undo the pinned node
            this.Model.CurrentWorkspace.Undo();
            Assert.IsNull(noteView.ViewModel.Model.PinnedNode);
        }
    }
}
