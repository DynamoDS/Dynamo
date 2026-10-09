using CoreNodeModels.Input;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests
{
    /// <summary>
    /// Undo and redo count as the person touching a node: they clear
    /// <see cref="NodeModel.IsRecentlyModifiedByAI"/> on every node they change (value, name,
    /// lacing, preview, freeze and position edits).
    /// </summary>
    [TestFixture]
    class AIHighlightUndoRedoTests : DynamoModelTestBase
    {
        [SetUp]
        public void UseManualRun()
        {
            if (CurrentDynamoModel.CurrentWorkspace is HomeWorkspaceModel home)
            {
                home.RunSettings.RunType = RunType.Manual;
            }
        }

        private NodeModel AddNode(NodeModel node)
        {
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(node, 0, 0, false, false));
            return node;
        }

        private NodeModel AddAddNode()
        {
            return AddNode(new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+")));
        }

        /// <summary>
        /// What DynamoMCP does: run the edit as a normal command, then turn the highlight on.
        /// </summary>
        private void AssistantEdit(NodeModel node, string property, string value)
        {
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(node.GUID, property, value));
            node.IsRecentlyModifiedByAI = true;
        }

        private void Undo()
        {
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Undo));
        }

        private void Redo()
        {
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Redo));
        }

        [Test]
        [Category("UnitTests")]
        [TestCase("Name", "Renamed")]
        [TestCase("ArgumentLacing", "Longest")]
        [TestCase("IsVisible", "false")]
        [TestCase("IsFrozen", "true")]
        [TestCase("Position", "300;200")]
        public void UndoAndRedoBothClearHighlight(string property, string value)
        {
            var node = AddAddNode();

            AssistantEdit(node, property, value);
            Assert.IsTrue(node.IsRecentlyModifiedByAI);

            Undo();
            Assert.IsFalse(node.IsRecentlyModifiedByAI, "undo should clear the highlight");

            // Turn it back on so redo has something to clear.
            node.IsRecentlyModifiedByAI = true;

            Redo();
            Assert.IsFalse(node.IsRecentlyModifiedByAI, "redo should clear the highlight");
        }

        [Test]
        [Category("UnitTests")]
        public void UndoAndRedoBothClearHighlightForAValueEdit()
        {
            var node = AddNode(new DoubleInput { Value = "1" });

            AssistantEdit(node, "Value", "5");
            Assert.IsTrue(node.IsRecentlyModifiedByAI);

            Undo();
            Assert.IsFalse(node.IsRecentlyModifiedByAI, "undo should clear the highlight");

            // Turn it back on so redo has something to clear.
            node.IsRecentlyModifiedByAI = true;

            Redo();
            Assert.IsFalse(node.IsRecentlyModifiedByAI, "redo should clear the highlight");
        }

        [Test]
        [Category("UnitTests")]
        public void UndoLeavesHighlightOnNodesItDoesNotTouch()
        {
            var edited = AddAddNode();
            var other = AddAddNode();
            other.IsRecentlyModifiedByAI = true;

            AssistantEdit(edited, "Position", "300;200");

            Undo();
            Assert.IsFalse(edited.IsRecentlyModifiedByAI);
            Assert.IsTrue(other.IsRecentlyModifiedByAI, "undo must only clear nodes it changes");
        }
    }
}
