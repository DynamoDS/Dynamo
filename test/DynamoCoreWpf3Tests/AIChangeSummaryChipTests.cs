using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
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
using WpfResources = Dynamo.Wpf.Properties.Resources;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Tests for the AI summary chip: its sentence, when it shows, Show all and Dismiss (DYN-11011), and the
    /// stepper and Undo all (DYN-11012).
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

        // Placed with an undo step labelled with the turn, as DynamoMCP does
        private NodeModel PlaceByAssistantUndoably(string turn = TurnA, double x = 0, double y = 0)
        {
            var node = new DoubleInput();
            using (Workspace.BeginAIEdit(turn))
            {
                ViewModel.Model.ExecuteCommand(new DynamoModel.CreateNodeCommand(node, x, y, false, false));
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, turn);
            }

            DispatcherUtil.DoEvents();
            return node;
        }

        private void DeleteByAssistant(NodeModel node, string turn = TurnA)
        {
            Workspace.RecordAINodeDeleted(node.GUID, turn);
            Workspace.RemoveAndDisposeNode(node);
        }

        private void DeleteByAssistantUndoably(NodeModel node, string turn = TurnA)
        {
            using (Workspace.BeginAIEdit(turn))
            {
                Workspace.RecordAINodeDeleted(node.GUID, turn);
                ViewModel.Model.ExecuteCommand(new DynamoModel.DeleteModelCommand(node.GUID));
            }

            DispatcherUtil.DoEvents();
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

        [Test]
        [Category("UnitTests")]
        [SetUICulture("en-US")]
        [TestCase(0, 2, 0, 1, "Autodesk Assistant changed 2 nodes · removed 1 connection")]
        [TestCase(0, 2, 1, 3, "Autodesk Assistant changed 2 nodes · deleted 1 · removed 3 connections")]
        [TestCase(0, 0, 0, 1, "Autodesk Assistant removed 1 connection")]
        [TestCase(0, 0, 0, 3, "Autodesk Assistant removed 3 connections")]
        public void SentenceCountsRemovedConnections(int added, int changed, int deleted, int removed, string expected)
        {
            Assert.AreEqual(expected, AIChangeSummaryChipViewModel.FormatMessage(added, changed, deleted, removed));
        }

        [Test]
        [Category("UnitTests")]
        [SetUICulture("en-US")]
        public void ConnectionsRemovedWithADeletedNodeAreNotCounted()
        {
            var kept = AddNumberNode();
            var feeder = AddNumberNode(0, 200);
            var doomed = AddNumberNode(0, 400);

            Workspace.RecordAINodeChange(kept.GUID, AIChangeKind.Modified, TurnA, "x", "Number", null);
            Workspace.RecordAIConnectorRemoved(feeder.GUID, 0, kept.GUID, 0, TurnA);
            // As DynamoMCP reports a deletion: the node's connections, then the node
            Workspace.RecordAIConnectorRemoved(doomed.GUID, 0, kept.GUID, 1, TurnA);
            DeleteByAssistant(doomed);

            Assert.AreEqual("Autodesk Assistant changed 1 node · deleted 1 · removed 1 connection", Chip.Message);
        }

        #endregion

        #region When the chip shows

        [Test]
        [Category("UnitTests")]
        [SetUICulture("en-US")]
        public void ChipShowsForATurnThatOnlyRemovedConnections()
        {
            var start = AddNumberNode();
            var end = AddNumberNode(0, 200);

            Workspace.RecordAIConnectorRemoved(start.GUID, 0, end.GUID, 0, TurnA);

            Assert.IsTrue(Chip.IsVisible);
            Assert.AreEqual("Autodesk Assistant removed 1 connection", Chip.Message);
            Assert.IsFalse(Chip.CanShow, "No node to step to or show.");
        }

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
        public void NodesThePersonEditsDropOutOfTheChip()
        {
            var edited = PlaceByAssistant(x: 0);
            PlaceByAssistant(x: 400);
            DeleteByAssistant(AddNumberNode());
            Assert.AreEqual("Autodesk Assistant added 2 nodes · deleted 1", Chip.Message);

            edited.OnNodeModified();

            Assert.IsNull(edited.AIChange, "The person's edit clears the marker.");
            Assert.AreEqual("Autodesk Assistant added 1 node · deleted 1", Chip.Message, "Deletions stay as history.");
            Assert.AreEqual("– of 1", Chip.StepText);
        }

        [Test]
        [Category("UnitTests")]
        [SetUICulture("en-US")]
        public void ClearingAMarkerWithEscDropsTheNode()
        {
            var cleared = PlaceByAssistant(x: 0);
            PlaceByAssistant(x: 400);

            // What Esc does to the hovered or selected node
            cleared.ClearAIChange();

            Assert.AreEqual("Autodesk Assistant added 1 node", Chip.Message);
            Assert.AreEqual("– of 1", Chip.StepText);
        }

        [Test]
        [Category("UnitTests")]
        public void ChipClosesOnceThePersonHasTakenOverAllTheTurnsNodes()
        {
            var first = PlaceByAssistant();
            var second = PlaceByAssistant();
            DeleteByAssistant(AddNumberNode());

            first.OnNodeModified();
            second.OnNodeModified();

            Assert.IsFalse(Chip.IsVisible, "The person has acknowledged the turn and it cannot be undone.");
        }

        [Test]
        [Category("UnitTests")]
        public void ChipClosesWhenNothingIsLeftToDescribeEvenIfUndoAllIsAvailable()
        {
            var placed = PlaceByAssistantUndoably();
            Assert.IsTrue(Chip.CanUndoAll);

            placed.ClearAIChange();

            Assert.IsFalse(Chip.IsVisible);
        }

        [Test]
        [Category("UnitTests")]
        [SetUICulture("en-US")]
        public void DeletionsKeepTheChipWhileUndoAllIsAvailable()
        {
            var placed = PlaceByAssistantUndoably();
            DeleteByAssistantUndoably(AddNumberNode(0, 300));
            Assert.IsTrue(Chip.CanUndoAll);

            placed.ClearAIChange();

            var chip = Chip;
            Assert.IsTrue(chip.IsVisible, "Undo all can still bring the deleted node back.");
            Assert.AreEqual("Autodesk Assistant deleted 1 node", chip.Message);
            Assert.IsFalse(chip.CanShow, "No marked node to show or step to.");
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
        public void ChipClosesOnceNoneOfTheTurnsNodesAreLeftAndItCannotBeUndone()
        {
            var placed = PlaceByAssistant();
            DeleteByAssistant(AddNumberNode());
            Assert.IsTrue(Chip.IsVisible);

            Workspace.RemoveAndDisposeNode(placed);

            Assert.IsFalse(Chip.IsVisible, "Nothing is left to step to, show or undo.");
        }

        [Test]
        [Category("UnitTests")]
        [SetUICulture("en-US")]
        public void ChipStaysClosedWhenThePersonUndoesDeletingTheTurnsNodes()
        {
            var placed = PlaceByAssistantUndoably();

            ViewModel.Model.ExecuteCommand(new DynamoModel.DeleteModelCommand(placed.GUID));
            Assert.IsFalse(Chip.IsVisible);

            ViewModel.Model.ExecuteCommand(new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Undo));

            var restored = Workspace.Nodes.Single(n => n.GUID == placed.GUID);
            Assert.IsNull(restored.AIChange, "Undo does not bring the marker back.");
            Assert.IsFalse(Chip.IsVisible, "So there is nothing left to describe.");
        }

        [Test]
        [Category("UnitTests")]
        public void ChipStaysWhileSomeOfTheTurnsNodesAreLeft()
        {
            var kept = PlaceByAssistantUndoably();
            var removed = PlaceByAssistantUndoably(x: 400);

            ViewModel.Model.ExecuteCommand(new DynamoModel.DeleteModelCommand(removed.GUID));

            Assert.IsTrue(Chip.IsVisible);
            Assert.IsTrue(Chip.CanShow, "The stepper goes through the node that is left.");
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
        public void ShowFramesAndGlowsOnlyTheMarkedNodes()
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
            Assert.AreEqual(0, zoom.Offset.X);
            Assert.AreEqual(0, zoom.Offset.Y);
            Assert.AreEqual(untouched.Width, zoom.FocusWidth, "Only the marked node is framed, not the edited one.");
            Assert.AreEqual(untouched.Height, zoom.FocusHeight);

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

        #region Stepper (DYN-11012)

        [Test]
        [Category("UnitTests")]
        [SetUICulture("en-US")]
        public void StepperGoesLeftToRightAndWraps()
        {
            // Touched right, left, middle; two in the middle column, the upper one touched last
            var right = PlaceByAssistant(x: 800, y: 0);
            var left = PlaceByAssistant(x: 0, y: 0);
            var middleLower = PlaceByAssistant(x: 400, y: 300);
            var middleUpper = PlaceByAssistant(x: 400, y: 0);
            var chip = Chip;
            Assert.AreEqual("– of 4", chip.StepText, "No step yet.");

            var centred = new List<NodeModel>();
            var texts = new List<string>();
            Action<NodeModel> capture = node => centred.Add(node);
            ViewModel.CurrentSpaceViewModel.RequestCenterViewOnStepNode += capture;
            try
            {
                for (var i = 0; i < 5; i++)
                {
                    chip.NextCommand.Execute(null);
                    texts.Add(chip.StepText);
                }
            }
            finally
            {
                ViewModel.CurrentSpaceViewModel.RequestCenterViewOnStepNode -= capture;
            }

            CollectionAssert.AreEqual(new[] { "1 of 4", "2 of 4", "3 of 4", "4 of 4", "1 of 4" }, texts, "Wraps after the last node.");
            CollectionAssert.AreEqual(new[] { left, middleUpper, middleLower, right, left }, centred,
                "Left to right, then top to bottom, whatever order the Assistant touched them in.");
        }

        [Test]
        [Category("UnitTests")]
        [TestCase(0.6, 200, 100, 1.0, TestName = "StepZoom_ZoomsInTo100Percent")]
        [TestCase(1.8, 200, 100, 1.8, TestName = "StepZoom_KeepsAHigherZoom")]
        [TestCase(3.0, 600, 400, 1.5, TestName = "StepZoom_ZoomsOutUntilTheNodeFits")]
        [TestCase(0.6, 2000, 1000, 0.5, TestName = "StepZoom_GoesBelow100PercentForANodeTooBigToFit")]
        [TestCase(5.0, 10, 10, WorkspaceViewModel.ZOOM_MAXIMUM, TestName = "StepZoom_StaysWithinTheZoomLimits")]
        public void StepZoom(double currentZoom, double nodeWidth, double nodeHeight, double expected)
        {
            // A 1000 x 600 fit area
            Assert.AreEqual(expected, WorkspaceViewModel.StepZoom(currentZoom, nodeWidth, nodeHeight, 1000, 600), 1e-9);
        }

        [Test]
        [Category("UnitTests")]
        public void SteppingCentresTheNodeAndKeepsAHigherZoom()
        {
            var node = PlaceByAssistant(x: 3000, y: 2000);
            var space = ViewModel.CurrentSpaceViewModel;
            space.Zoom = 1.8;

            Chip.NextCommand.Execute(null);
            DispatcherUtil.DoEvents();

            var workspaceView = View.ChildrenOfType<WorkspaceView>().First();
            var chipView = workspaceView.ChildrenOfType<AIChangeSummaryChip>().First();
            var nodeView = NodeViewWithGuid(node.GUID.ToString());
            var nodeCentre = nodeView.TranslatePoint(new Point(nodeView.ActualWidth / 2, nodeView.ActualHeight / 2), workspaceView);
            var chipTop = chipView.TranslatePoint(new Point(0, 0), workspaceView).Y;

            Assert.AreEqual(1.8, space.Zoom, 1e-9, "A zoom above 100% is kept.");
            Assert.AreEqual(workspaceView.ActualWidth / 2, nodeCentre.X, 5, "Centred across the canvas.");
            Assert.Less(nodeCentre.Y, chipTop, "Above the chip.");
        }

        [Test]
        [Category("UnitTests")]
        [SetUICulture("en-US")]
        public void PreviousFromTheStartGoesToTheLastNode()
        {
            PlaceByAssistant(x: 0);
            PlaceByAssistant(x: 400);

            Chip.PreviousCommand.Execute(null);

            Assert.AreEqual("2 of 2", Chip.StepText);
        }

        [Test]
        [Category("UnitTests")]
        public void SteppingGivesTheHaloOneSlowBreath()
        {
            var marked = PlaceByAssistant();

            Chip.NextCommand.Execute(null);

            var glow = ViewModelOf(marked).AIHaloGlow;
            Assert.IsTrue(glow.IsPulse);
            Assert.AreEqual(AIHaloGlow.BreathLength, glow.Duration, "One 2 s breath.");
        }

        [Test]
        [Category("UnitTests")]
        [SetUICulture("en-US")]
        [TestCase(1, 800, "2 of 2", TestName = "StepperCarriesOnWhenItsNodeIsEdited_Next")]
        [TestCase(-1, 0, "1 of 2", TestName = "StepperCarriesOnWhenItsNodeIsEdited_Previous")]
        public void StepperCarriesOnWhenItsNodeIsEdited(int direction, double expectedX, string expectedText)
        {
            PlaceByAssistant(x: 0);
            var edited = PlaceByAssistant(x: 400);
            PlaceByAssistant(x: 800);
            var chip = Chip;
            chip.NextCommand.Execute(null);
            chip.NextCommand.Execute(null);
            Assert.AreEqual("2 of 3", chip.StepText);

            edited.OnNodeModified();
            chip = Chip;
            Assert.AreEqual("– of 2", chip.StepText, "The node it was on is the person's now.");

            NodeModel centred = null;
            Action<NodeModel> capture = node => centred = node;
            ViewModel.CurrentSpaceViewModel.RequestCenterViewOnStepNode += capture;
            try
            {
                if (direction > 0) chip.NextCommand.Execute(null);
                else chip.PreviousCommand.Execute(null);
            }
            finally
            {
                ViewModel.CurrentSpaceViewModel.RequestCenterViewOnStepNode -= capture;
            }

            Assert.AreEqual(expectedX, centred.X, "The neighbour of the node it was on, with none skipped.");
            Assert.AreEqual(expectedText, chip.StepText);
        }

        [Test]
        [Category("UnitTests")]
        public void EveryChipControlCanBeReachedWithTheKeyboard()
        {
            PlaceByAssistantUndoably();
            Assert.IsTrue(Chip.IsVisible);

            var chipView = View.ChildrenOfType<AIChangeSummaryChip>().First();
            var buttons = chipView.ChildrenOfType<Button>().ToList();

            Assert.AreEqual(5, buttons.Count, "Previous, Next, Show all, Undo all and Dismiss.");
            foreach (var button in buttons)
            {
                Assert.IsTrue(button.Focusable && button.IsTabStop, "Every control takes keyboard focus.");
                Assert.IsInstanceOf<string>(button.Content, "Every control has a text label.");
            }
        }

        [Test]
        [Category("UnitTests")]
        public void ArrowsHaveScreenReaderNames()
        {
            PlaceByAssistant();
            Assert.IsTrue(Chip.IsVisible);

            var chipView = View.ChildrenOfType<AIChangeSummaryChip>().First();
            var names = chipView.ChildrenOfType<Button>()
                .Where(button => (string)button.Content == "\u2039" || (string)button.Content == "\u203A")
                .Select(AutomationProperties.GetName)
                .ToList();

            CollectionAssert.AreEqual(new[] { WpfResources.AIChangeSummaryChipPrevious, WpfResources.AIChangeSummaryChipNext }, names,
                "A screen reader says what the arrows do, not the arrow characters.");
        }

        #endregion

        #region Undo all (DYN-11012)

        [Test]
        [Category("UnitTests")]
        public void UndoAllRevertsTheTurnAndClosesTheChip()
        {
            var placed = PlaceByAssistantUndoably();
            var chip = Chip;
            Assert.IsTrue(chip.CanUndoAll);

            chip.UndoAllCommand.Execute(null);

            Assert.IsFalse(Workspace.Nodes.Any(n => n.GUID == placed.GUID), "The turn is undone.");
            Assert.IsFalse(Chip.IsVisible, "The chip closes.");
        }

        [Test]
        [Category("UnitTests")]
        public void UndoAllExplainsWhyItIsUnavailableAfterAPersonEdit()
        {
            PlaceByAssistantUndoably();
            ViewModel.Model.ExecuteCommand(new DynamoModel.CreateNodeCommand(new DoubleInput(), 300, 0, false, false));

            var chip = Chip;
            Assert.IsFalse(chip.CanUndoAll);
            Assert.IsFalse(chip.UndoAllCommand.CanExecute(null));
            Assert.AreEqual(WpfResources.AIChangeSummaryChipUndoAllEditedSince, chip.UndoAllToolTip);
        }

        [Test]
        [Category("UnitTests")]
        public void UndoAllIsHiddenWhenTheAssistantDoesNotLabelItsEdits()
        {
            // As an older DynamoMCP runs an edit
            var node = new DoubleInput();
            using (Workspace.BeginAIEdit())
            {
                ViewModel.Model.ExecuteCommand(new DynamoModel.CreateNodeCommand(node, 0, 0, false, false));
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            }

            var chip = Chip;
            Assert.IsTrue(chip.IsVisible, "The rest of the chip still works.");
            Assert.IsFalse(chip.ShowUndoAll);
            Assert.IsFalse(chip.CanUndoAll);
        }

        [Test]
        [Category("UnitTests")]
        public void UndoAllWaitsForTheAssistantToFinish()
        {
            using (Workspace.BeginUndoActionGroup())
            {
                PlaceByAssistantUndoably();
                Assert.IsFalse(Chip.CanUndoAll);
                Assert.AreEqual(WpfResources.AIChangeSummaryChipUndoAllWorking, Chip.UndoAllToolTip);
            }

            Assert.IsTrue(Chip.CanUndoAll, "Available once the Assistant's undo group closes.");
            Assert.AreEqual(WpfResources.AIChangeSummaryChipUndoAllToolTip, Chip.UndoAllToolTip);
        }

        #endregion
    }
}
