using System.Linq;
using Dynamo.Graph.Nodes;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    /// <summary>Workspace context menu "Clear AI Highlights" command (DYN-11002).</summary>
    public class AIHighlightsContextMenuTests : DynamoTestUIBase
    {
        private const string Turn = "turn-a";

        [Test]
        public void ClearAIHighlightsIsOfferedOnlyWhileANodeIsHighlighted()
        {
            Open(@"UI/ZoomNodeColorStates.dyn");
            var workspace = ViewModel.CurrentSpaceViewModel;

            Assert.IsFalse(workspace.ClearAIHighlightsCommand.CanExecute(null));

            var node = workspace.Model.Nodes.First();
            workspace.Model.RecordAINodeChange(node.GUID, AIChangeKind.Placed, Turn);

            Assert.IsTrue(workspace.ClearAIHighlightsCommand.CanExecute(null));
        }

        [Test]
        public void ClearAIHighlightsClearsEveryNodeWithoutDirtyingTheGraphOrAddingAnUndoStep()
        {
            Open(@"UI/ZoomNodeColorStates.dyn");
            var workspace = ViewModel.CurrentSpaceViewModel;
            var highlighted = workspace.Model.Nodes.Take(2).ToList();
            Assert.AreEqual(2, highlighted.Count, "The test graph needs at least two nodes.");

            workspace.Model.RecordAINodeChange(highlighted[0].GUID, AIChangeKind.Placed, Turn);
            workspace.Model.RecordAINodeChange(highlighted[1].GUID, AIChangeKind.Modified, Turn, "Value", "1", "2");
            workspace.Model.HasUnsavedChanges = false;
            var couldUndo = workspace.Model.UndoRecorder.CanUndo;

            workspace.ClearAIHighlightsCommand.Execute(null);

            Assert.IsFalse(workspace.Model.Nodes.Any(n => n.IsRecentlyModifiedByAI));
            Assert.IsFalse(workspace.ClearAIHighlightsCommand.CanExecute(null));
            Assert.IsFalse(workspace.Model.HasUnsavedChanges);
            Assert.AreEqual(couldUndo, workspace.Model.UndoRecorder.CanUndo);
            CollectionAssert.AreEquivalent(highlighted.Select(n => n.GUID), workspace.Model.GetAITurnSummary(Turn).NodeIds,
                "The turn's own history is kept.");
        }
    }
}
