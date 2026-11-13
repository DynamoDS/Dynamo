using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using CoreNodeModels.Input;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.UI.Controls;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Views;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Tests for the AI summary chip (DYN-11011): its sentence, when it shows, Show and Dismiss.
    /// </summary>
    public class AIChangeSummaryChipTests : DynamoTestUIBase
    {
        private const string TurnA = "turn-a";
        private const string TurnB = "turn-b";

        private WorkspaceModel Workspace => ViewModel.Model.CurrentWorkspace;

        private AIChangeSummaryChipViewModel Chip
        {
            get
            {
                // The chip refreshes on the UI thread after a burst of changes
                DispatcherUtil.DoEvents();
                return ViewModel.CurrentSpaceViewModel.AIChangeSummaryChip;
            }
        }

        private NodeModel AddNumberNode(double x = 0, double y = 0)
        {
            var node = new DoubleInput { X = x, Y = y };
            Workspace.AddAndRegisterNode(node, false);
            DispatcherUtil.DoEvents();
            return node;
        }

        private NodeModel PlaceByAssistant(string turn = TurnA, double x = 0, double y = 0)
        {
            var node = AddNumberNode(x, y);
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, turn);
            return node;
        }

        private void DeleteByAssistant(NodeModel node, string turn = TurnA)
        {
            Workspace.RecordAINodeDeleted(node.GUID, turn);
            Workspace.RemoveAndDisposeNode(node);
        }

        private NodeViewModel ViewModelOf(NodeModel node)
        {
            return ViewModel.CurrentSpaceViewModel.Nodes.Single(n => n.NodeModel == node);
        }

        #region Sentence

        [Test]
        [Category("UnitTests")]
        [SetUICulture("en-US")]
        [TestCase(3, 9, 2, "Autodesk Assistant added 3 nodes · changed 9 · deleted 2")]
        [TestCase(2, 0, 1, "Autodesk Assistant added 2 nodes · deleted 1")]
        [TestCase(1, 0, 0, "Autodesk Assistant added 1 node")]
        [TestCase(0, 9, 2, "Autodesk Assistant changed 9 nodes · deleted 2")]
        [TestCase(0, 1, 0, "Autodesk Assistant changed 1 node")]
        [TestCase(0, 0, 2, "Autodesk Assistant deleted 2 nodes")]
        [TestCase(0, 0, 1, "Autodesk Assistant deleted 1 node")]
        public void SentenceNamesOnlyTheGroupsThatChanged(int added, int changed, int deleted, string expected)
        {
            Assert.AreEqual(expected, AIChangeSummaryChipViewModel.FormatMessage(added, changed, deleted));
        }

        [Test]
        [Category("UnitTests")]
        [SetUICulture("en-US")]
        public void ChipCountsAddedChangedAndDeletedNodes()
        {
            var modified = AddNumberNode();
            var moved = AddNumberNode();
            var deleted = AddNumberNode();

            PlaceByAssistant();
            PlaceByAssistant();
            Workspace.RecordAINodeChange(modified.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            Workspace.RecordAINodeChange(moved.GUID, AIChangeKind.Moved, TurnA, null, "0, 0", "100, 50");
            DeleteByAssistant(deleted);

            Assert.IsTrue(Chip.IsVisible);
            Assert.AreEqual("Autodesk Assistant added 2 nodes · changed 2 · deleted 1", Chip.Message,
                "Moved nodes count as changed.");
        }

        [Test]
        [Category("UnitTests")]
        [SetUICulture("en-US")]
        public void NodePlacedThenChangedInTheSameTurnCountsAsAdded()
        {
            var node = PlaceByAssistant();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "0", "5");

            Assert.AreEqual("Autodesk Assistant added 1 node", Chip.Message);
        }

        #endregion

        #region When the chip shows

        [Test]
        [Category("UnitTests")]
        public void NoChipWithoutAnAssistantTurn()
        {
            AddNumberNode();

            Assert.IsFalse(Chip.IsVisible);
        }

        [Test]
        [Category("UnitTests")]
        [SetUICulture("en-US")]
        public void ChipStaysWhenAPersonEditsTheTurnsNodes()
        {
            var first = PlaceByAssistant();
            var second = PlaceByAssistant();
            var before = Chip.Message;

            first.OnNodeModified();
            second.OnNodeModified();

            Assert.IsNull(first.AIChange, "The person's edits clear the markers.");
            Assert.IsTrue(Chip.IsVisible, "The chip is the turn's history and stays until Dismiss or the next turn.");
            Assert.AreEqual(before, Chip.Message, "The sentence does not change.");
            Assert.IsTrue(Chip.CanShow, "Show still frames the turn's nodes.");
        }

        [Test]
        [Category("UnitTests")]
        [SetUICulture("en-US")]
        public void NextTurnReplacesTheChip()
        {
            PlaceByAssistant(TurnA);
            PlaceByAssistant(TurnA);
            var existing = AddNumberNode();

            Workspace.RecordAINodeChange(existing.GUID, AIChangeKind.Modified, TurnB, "Value", "0", "3");

            Assert.AreEqual("Autodesk Assistant changed 1 node", Chip.Message);
        }

        [Test]
        [Category("UnitTests")]
        public void DeletionsOnlyShowTheChipWithoutShow()
        {
            DeleteByAssistant(AddNumberNode());

            Assert.IsTrue(Chip.IsVisible, "The chip is the only place deleted nodes show up.");
            Assert.IsFalse(Chip.CanShow, "There is nothing to frame.");
        }

        [Test]
        [Category("UnitTests")]
        [SetUICulture("en-US")]
        public void ShowIsHiddenOnceNoneOfTheTurnsNodesAreLeft()
        {
            var placed = PlaceByAssistant();
            DeleteByAssistant(AddNumberNode());
            var before = Chip.Message;

            Workspace.RemoveAndDisposeNode(placed);

            Assert.IsTrue(Chip.IsVisible);
            Assert.AreEqual(before, Chip.Message);
            Assert.IsFalse(Chip.CanShow);
        }

        [Test]
        [Category("UnitTests")]
        public void NoChipAfterSaveAndReopen()
        {
            PlaceByAssistant();
            Assert.IsTrue(Chip.IsVisible);

            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".dyn");
            try
            {
                Workspace.Save(path);
                ViewModel.OpenCommand.Execute(path);

                Assert.IsFalse(Chip.IsVisible, "The change record is never saved, so nothing comes back.");
            }
            finally
            {
                File.Delete(path);
            }
        }

        #endregion

        #region Dismiss

        [Test]
        [Category("UnitTests")]
        public void DismissClearsTheHighlightsWithoutTouchingTheGraph()
        {
            var node = PlaceByAssistant();
            DeleteByAssistant(AddNumberNode());
            Workspace.HasUnsavedChanges = false;
            var nodeCount = Workspace.Nodes.Count();
            var canUndo = Workspace.CanUndo;
            Assert.IsNotNull(ViewModelOf(node).AIHaloGlow, "The placed node has a glow before Dismiss.");

            Chip.DismissCommand.Execute(null);

            Assert.IsFalse(Chip.IsVisible);
            Assert.IsNull(node.AIChange, "Every AI highlight is cleared.");
            Assert.IsNull(ViewModelOf(node).AIHaloGlow, "The halo is cleared too, not just the change record.");
            Assert.AreEqual(nodeCount, Workspace.Nodes.Count(), "The graph is unchanged.");
            Assert.IsFalse(Workspace.HasUnsavedChanges, "Dismiss never marks the graph unsaved.");
            Assert.AreEqual(canUndo, Workspace.CanUndo, "Dismiss adds no undo step.");
        }

        #endregion

        #region Show

        [Test]
        [Category("UnitTests")]
        public void ShowFramesAllTheTurnsNodesButGlowsOnlyTheUneditedOnes()
        {
            var untouched = PlaceByAssistant(x: 0, y: 0);
            var edited = PlaceByAssistant(x: 600, y: 400);
            edited.OnNodeModified();
            DispatcherUtil.DoEvents();
            var glowBefore = ViewModelOf(untouched).AIHaloGlow;

            ZoomEventArgs zoom = null;
            var glowWaitingRaised = false;
            WorkspaceViewModel.ZoomEventHandler capture = (sender, e) => zoom = e as ZoomEventArgs;
            Action onGlowWaiting = () => glowWaitingRaised = true;
            var chip = Chip;
            ViewModel.CurrentSpaceViewModel.RequestZoomToFitView += capture;
            ViewModel.CurrentSpaceViewModel.AIHaloGlowWaiting += onGlowWaiting;
            try
            {
                chip.ShowCommand.Execute(null);
            }
            finally
            {
                ViewModel.CurrentSpaceViewModel.RequestZoomToFitView -= capture;
                ViewModel.CurrentSpaceViewModel.AIHaloGlowWaiting -= onGlowWaiting;
            }

            Assert.IsNotNull(zoom, "Show zooms to fit.");
            Assert.AreEqual(0, zoom.Offset.X, "Framed from the leftmost node...");
            Assert.AreEqual(0, zoom.Offset.Y, "...and the topmost node,");
            Assert.AreEqual(600 + edited.Width, zoom.FocusWidth, "...to the edited node, which is framed too.");
            Assert.AreEqual(400 + edited.Height, zoom.FocusHeight);

            Assert.IsNotNull(ViewModelOf(untouched).AIHaloGlow);
            Assert.AreNotSame(glowBefore, ViewModelOf(untouched).AIHaloGlow, "The untouched node glows again.");
            Assert.IsNull(ViewModelOf(edited).AIHaloGlow, "The edited node belongs to the person and does not glow.");

            // The new glow must actually start, even when the view would not move
            Assert.IsTrue(glowWaitingRaised, "Show asks the view to check for glows to start.");
            var workspaceView = View.ChildrenOfType<WorkspaceView>().First();
            workspaceView.CheckAIHaloVisibility();
            Assert.IsTrue(ViewModelOf(untouched).AIHaloGlow.IsStarted, "The replayed glow starts once its node is on screen.");
        }

        [Test]
        [Category("UnitTests")]
        public void FitKeepsTheChipAreaFree()
        {
            // Stacked nodes, so the fit is limited by the view's height
            PlaceByAssistant(x: 0, y: 0);
            var lowest = PlaceByAssistant(x: 0, y: 3000);
            Assert.IsTrue(Chip.IsVisible);

            Chip.ShowCommand.Execute(null);
            DispatcherUtil.DoEvents();

            var workspaceView = View.ChildrenOfType<WorkspaceView>().First();
            var chipView = workspaceView.ChildrenOfType<AIChangeSummaryChip>().First();
            var nodeView = NodeViewWithGuid(lowest.GUID.ToString());

            var chipTop = chipView.TranslatePoint(new Point(0, 0), workspaceView).Y;
            var nodeBottom = nodeView.TranslatePoint(new Point(0, nodeView.ActualHeight), workspaceView).Y;

            Assert.Greater(chipView.ActualHeight, 0, "The chip is on screen.");
            Assert.LessOrEqual(nodeBottom, chipTop, "The lowest framed node sits above the chip.");
        }

        #endregion

        #region Keyboard

        [Test]
        [Category("UnitTests")]
        [SetUICulture("en-US")]
        public void ChipSentenceHasANameNarratorCanRead()
        {
            PlaceByAssistant();
            var chipView = View.ChildrenOfType<WorkspaceView>().First().ChildrenOfType<AIChangeSummaryChip>().First();

            Assert.AreEqual("Autodesk Assistant added 1 node", PeerName(chipView.MessageButton));
        }

        private static string PeerName(UIElement element)
        {
            var peer = UIElementAutomationPeer.CreatePeerForElement(element);
            Assert.IsNotNull(peer, "Narrator has no name to read.");
            Assert.AreEqual(AutomationControlType.Button, peer.GetAutomationControlType());
            return peer.GetName();
        }

        #endregion
    }
}
