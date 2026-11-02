using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using CoreNodeModels.Input;
using Dynamo.Graph;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using DefineDataNode = CoreNodeModels.DefineData;
using WatchNode = CoreNodeModels.Watch;

namespace Dynamo.Tests
{
    /// <summary>
    /// Tests for the AI change record (DYN-10998): what an AI Assistant turn did to each node, each
    /// turn's own history and deletion totals, and when a node's current record is cleared.
    /// </summary>
    [TestFixture]
    class AIChangeRecordTests : DynamoModelTestBase
    {
        private const string TurnA = "turn-a";
        private const string TurnB = "turn-b";

        private WorkspaceModel Workspace => CurrentDynamoModel.CurrentWorkspace;

        private NodeModel AddNumberNode()
        {
            return AddNode(new DoubleInput());
        }

        private T AddNode<T>(T node) where T : NodeModel
        {
            if (Workspace is HomeWorkspaceModel home)
            {
                home.RunSettings.RunType = RunType.Manual;
            }

            Workspace.AddAndRegisterNode(node, false);
            return node;
        }

        private (NodeModel before, WatchNode watch, NodeModel after) AddChainThroughInlineWatch()
        {
            var before = AddNumberNode();
            var watch = AddNode(new WatchNode());
            var after = AddNode(new WatchNode());
            Assert.IsNotNull(ConnectorModel.Make(before, watch, 0, 0));
            Assert.IsNotNull(ConnectorModel.Make(watch, after, 0, 0));
            return (before, watch, after);
        }

        /// <summary>Runs an edit as DynamoMCP does: inside a BeginAIEdit scope.</summary>
        private void AsAIEdit(Action edit)
        {
            using (Workspace.BeginAIEdit())
            {
                edit();
            }
        }

        #region What a turn records

        [Test]
        [Category("UnitTests")]
        public void TurnListsExactlyTheNodesItTouchedWithTheirKinds()
        {
            var placed = AddNumberNode();
            var modified = AddNumberNode();
            var moved = AddNumberNode();
            var untouched = AddNumberNode();

            Workspace.RecordAINodeChange(placed.GUID, AIChangeKind.Placed, TurnA);
            Workspace.RecordAINodeChange(modified.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            Workspace.RecordAINodeChange(moved.GUID, AIChangeKind.Moved, TurnA, null, "0, 0", "100, 50");

            var summary = Workspace.GetAITurnSummary(TurnA);
            Assert.IsNotNull(summary);
            CollectionAssert.AreEqual(new[] { placed.GUID, modified.GUID, moved.GUID }, summary.NodeIds);
            CollectionAssert.AreEqual(
                new[] { AIChangeKind.Placed, AIChangeKind.Modified, AIChangeKind.Moved },
                summary.Nodes.Select(n => n.Kind));

            Assert.AreEqual(AIChangeKind.Placed, placed.AIChange.Kind);
            Assert.AreEqual(AIChangeKind.Modified, modified.AIChange.Kind);
            Assert.AreEqual(AIChangeKind.Moved, moved.AIChange.Kind);
            Assert.IsTrue(placed.IsRecentlyModifiedByAI);
            Assert.IsNull(untouched.AIChange);
            Assert.IsFalse(untouched.IsRecentlyModifiedByAI);
        }

        [Test]
        [Category("UnitTests")]
        public void ModifiedNodeCarriesInputNameAndBeforeAndAfter()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "2", "3");

            var change = node.AIChange.InputChanges.Single();
            Assert.AreEqual("Value", change.InputName);
            Assert.AreEqual("1", change.Before, "Keeps the value from before the turn.");
            Assert.AreEqual("3", change.After, "Follows the latest value.");
            Assert.AreEqual(node.GUID, node.AIChange.NodeId);
            Assert.AreEqual(TurnA, node.AIChange.TurnId);
            Assert.AreEqual(1, node.AIChange.TurnNumber);
        }

        [Test]
        [Category("UnitTests")]
        public void SeveralInputsKeepTheOrderTheyWereFirstChanged()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "x", null, "A.out");
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "x", "A.out", "B.out");

            var inputs = node.AIChange.InputChanges;
            CollectionAssert.AreEqual(new[] { "x", "Value" }, inputs.Select(i => i.InputName));
            Assert.IsNull(inputs[0].Before);
            Assert.AreEqual("B.out", inputs[0].After);
        }

        [Test]
        [Category("UnitTests")]
        public void LongValuesAreStoredInFull()
        {
            var node = AddNumberNode();
            var before = new string('a', 5000);
            var after = new string('a', 4999) + "b";

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Script", before, after);

            var change = node.AIChange.InputChanges.Single();
            Assert.AreEqual(before, change.Before);
            Assert.AreEqual(after, change.After);
        }

        [Test]
        [Category("UnitTests")]
        public void ModifiedWithoutAnInputNameIsRejected()
        {
            var node = AddNumberNode();

            Assert.Throws<ArgumentException>(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA));
            Assert.Throws<ArgumentException>(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, string.Empty, "1", "2"));
            Assert.IsNull(node.AIChange);
        }

        [Test]
        [Category("UnitTests")]
        public void ChangeWithoutATurnIsRejected()
        {
            var node = AddNumberNode();

            Assert.Throws<ArgumentException>(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, null));
            Assert.Throws<ArgumentException>(() => Workspace.RecordAINodeDeleted(node.GUID, string.Empty));
            Assert.Throws<ArgumentException>(() => Workspace.RecordAIConnectorRemoved(Guid.NewGuid(), 0, Guid.NewGuid(), 0, null));
        }

        [Test]
        [Category("UnitTests")]
        public void UnknownAndTransientNodesAreNeverRecorded()
        {
            var transient = AddNumberNode();
            transient.IsTransient = true;

            Workspace.RecordAINodeChange(Guid.NewGuid(), AIChangeKind.Placed, TurnA);
            Workspace.RecordAINodeChange(transient.GUID, AIChangeKind.Placed, TurnA);

            Assert.IsNull(transient.AIChange);
            CollectionAssert.IsEmpty(Workspace.GetAITurnSummaries(), "A node that cannot be recorded creates no empty turn.");
        }

        #endregion

        #region Net effect of a turn

        [Test]
        [Category("UnitTests")]
        public void ValueChangedBackInTheSameTurnLeavesNoRecord()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "2", "1");

            Assert.IsNull(node.AIChange);
            CollectionAssert.IsEmpty(Workspace.GetAITurnSummary(TurnA).NodeIds);
        }

        [Test]
        [Category("UnitTests")]
        public void NodeMovedBackToWhereItStartedLeavesNoRecord()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Moved, TurnA, null, "0, 0", "10, 10");
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Moved, TurnA, null, "10, 10", "0, 0");

            Assert.IsNull(node.AIChange);
        }

        [Test]
        [Category("UnitTests")]
        public void PlacedWinsOverLaterChangesInTheSameTurn()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "0", "5");
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Moved, TurnA, null, "0, 0", "10, 10");

            Assert.AreEqual(AIChangeKind.Placed, node.AIChange.Kind);
            CollectionAssert.IsEmpty(node.AIChange.InputChanges);
        }

        [Test]
        [Category("UnitTests")]
        public void ModifiedWinsOverMovedAndAMoveSurvivesAValueChangedBack()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Moved, TurnA, null, "0, 0", "10, 10");
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            Assert.AreEqual(AIChangeKind.Modified, node.AIChange.Kind);

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "2", "1");
            Assert.AreEqual(AIChangeKind.Moved, node.AIChange.Kind);
        }

        #endregion

        #region Turns and their history

        [Test]
        [Category("UnitTests")]
        public void LaterTurnReplacesTheNodeRecordButTheEarlierTurnKeepsItsHistory()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnB, "Value", "0", "5");

            Assert.AreEqual(AIChangeKind.Modified, node.AIChange.Kind);
            Assert.AreEqual(TurnB, node.AIChange.TurnId);
            Assert.AreEqual(2, node.AIChange.TurnNumber);

            var turnA = Workspace.GetAITurnSummary(TurnA);
            Assert.AreEqual(1, turnA.TurnNumber);
            CollectionAssert.AreEqual(new[] { node.GUID }, turnA.NodeIds);
            Assert.AreEqual(AIChangeKind.Placed, turnA.Nodes.Single().Kind);
            CollectionAssert.AreEqual(new[] { TurnA, TurnB }, Workspace.GetAITurnSummaries().Select(t => t.TurnId));
        }

        [Test]
        [Category("UnitTests")]
        public void LaterTurnThatNetsToNothingLeavesTheEarlierTurnsRecord()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnB, "Value", "2", "3");
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnB, "Value", "3", "2");

            Assert.AreEqual(TurnA, node.AIChange?.TurnId);
            CollectionAssert.IsEmpty(Workspace.GetAITurnSummary(TurnB).NodeIds);
        }

        [Test]
        [Category("UnitTests")]
        public void RecordAPersonClearedIsNotBroughtBackByALaterTurn()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            node.OnNodeModified();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnB, "Value", "2", "3");
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnB, "Value", "3", "2");

            Assert.IsNull(node.AIChange);
        }

        [Test]
        [Category("UnitTests")]
        public void RecordAPersonClearedIsNotBroughtBackWhenTheSameTurnComesBack()
        {
            var node = AddNumberNode();

            // Turn A marks the node, then turn B takes it over.
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnB, "Value", "2", "3");

            // A person renames the node, which clears its record.
            node.Name = "Renamed";
            Assert.IsNull(node.AIChange);

            // Turn B comes back, marks the node again, then returns the value to where B started.
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnB, "Value", "3", "4");
            Assert.AreEqual(TurnB, node.AIChange?.TurnId);
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnB, "Value", "4", "2");

            Assert.IsNull(node.AIChange, "Turn A's mark, which the rename cleared, must not come back.");
            CollectionAssert.AreEqual(new[] { node.GUID }, Workspace.GetAITurnSummary(TurnA).NodeIds,
                "Turn A's own history is kept.");
        }

        [Test]
        [Category("UnitTests")]
        public void ClearAIChangesRemovesEverythingAndRestartsTurnNumbering()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            Workspace.ClearAIChanges();

            Assert.IsNull(node.AIChange);
            Assert.IsNull(Workspace.GetAITurnSummary(TurnA));
            CollectionAssert.IsEmpty(Workspace.GetAITurnSummaries());

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnB);
            Assert.AreEqual(1, node.AIChange.TurnNumber);
        }

        #endregion

        #region Deleted nodes and removed connections

        [Test]
        [Category("UnitTests")]
        public void DeletedNodesAreCountedOnceAndNotListed()
        {
            var existing = AddNumberNode();
            var modifiedThenDeleted = AddNumberNode();
            Workspace.RecordAINodeChange(modifiedThenDeleted.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");

            Workspace.RecordAINodeDeleted(existing.GUID, TurnA);
            Workspace.RecordAINodeDeleted(existing.GUID, TurnA);
            Workspace.RecordAINodeDeleted(Guid.NewGuid(), TurnA);
            Workspace.RecordAINodeDeleted(modifiedThenDeleted.GUID, TurnA);
            Workspace.RemoveAndDisposeNode(existing);
            Workspace.RemoveAndDisposeNode(modifiedThenDeleted);

            var summary = Workspace.GetAITurnSummary(TurnA);
            Assert.AreEqual(2, summary.DeletedNodeCount, "A node reported twice counts once; an unknown node is not counted.");
            CollectionAssert.IsEmpty(summary.NodeIds, "A deleted node is counted, not listed.");
        }

        [Test]
        [Category("UnitTests")]
        public void NodePlacedAndDeletedInTheSameTurnIsNeitherListedNorCounted()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            // A person's edit clears the node's record, but the turn still knows it placed the node.
            node.OnNodeModified();

            Workspace.RecordAINodeDeleted(node.GUID, TurnA);
            Workspace.RemoveAndDisposeNode(node);

            var summary = Workspace.GetAITurnSummary(TurnA);
            Assert.AreEqual(0, summary.DeletedNodeCount);
            CollectionAssert.IsEmpty(summary.NodeIds);
        }

        [Test]
        [Category("UnitTests")]
        public void NodePlacedInTheTurnAndReportedDeletedTwiceIsNotCounted()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            Workspace.RecordAINodeDeleted(node.GUID, TurnA);
            Workspace.RecordAINodeDeleted(node.GUID, TurnA);
            Workspace.RemoveAndDisposeNode(node);

            var summary = Workspace.GetAITurnSummary(TurnA);
            Assert.AreEqual(0, summary.DeletedNodeCount);
            CollectionAssert.IsEmpty(summary.NodeIds);
        }

        [Test]
        [Category("UnitTests")]
        public void RemovedConnectionsAreCountedAsTheTurnsNetEffect()
        {
            var a = Guid.NewGuid();
            var b = Guid.NewGuid();
            var c = Guid.NewGuid();

            // Existed before the turn and removed: counted, once.
            Workspace.RecordAIConnectorRemoved(a, 0, b, 0, TurnA);
            Workspace.RecordAIConnectorRemoved(a, 0, b, 0, TurnA);
            // Added and removed in the same turn: not counted.
            Workspace.RecordAIConnectorAdded(a, 0, c, 1, TurnA);
            Workspace.RecordAIConnectorRemoved(a, 0, c, 1, TurnA);
            // Removed and put back in the same turn: not counted.
            Workspace.RecordAIConnectorRemoved(b, 0, c, 0, TurnA);
            Workspace.RecordAIConnectorAdded(b, 0, c, 0, TurnA);

            Assert.AreEqual(1, Workspace.GetAITurnSummary(TurnA).RemovedConnectorCount);
        }

        [Test]
        [Category("UnitTests")]
        public void RepeatedConnectionReportsDoNotChangeTheCount()
        {
            var a = Guid.NewGuid();
            var b = Guid.NewGuid();
            var c = Guid.NewGuid();

            // Added in the turn, then reported removed twice: the net effect is still nothing.
            Workspace.RecordAIConnectorAdded(a, 0, b, 0, TurnA);
            Workspace.RecordAIConnectorRemoved(a, 0, b, 0, TurnA);
            Workspace.RecordAIConnectorRemoved(a, 0, b, 0, TurnA);
            Assert.AreEqual(0, Workspace.GetAITurnSummary(TurnA).RemovedConnectorCount);

            // Existed before the turn, removed, put back, then reported removed twice: counted once.
            Workspace.RecordAIConnectorRemoved(b, 0, c, 0, TurnA);
            Workspace.RecordAIConnectorAdded(b, 0, c, 0, TurnA);
            Workspace.RecordAIConnectorRemoved(b, 0, c, 0, TurnA);
            Workspace.RecordAIConnectorRemoved(b, 0, c, 0, TurnA);
            Assert.AreEqual(1, Workspace.GetAITurnSummary(TurnA).RemovedConnectorCount);
        }

        #endregion

        #region When a node's record is cleared

        [Test]
        [Category("UnitTests")]
        public void PersonEditingTheNodeClearsOnlyItsRecordAndNotTheTurnsHistory()
        {
            var edited = AddNumberNode();
            var other = AddNumberNode();
            Workspace.RecordAINodeChange(edited.GUID, AIChangeKind.Placed, TurnA);
            Workspace.RecordAINodeChange(other.GUID, AIChangeKind.Placed, TurnA);

            edited.OnNodeModified();

            Assert.IsNull(edited.AIChange);
            Assert.IsNotNull(other.AIChange, "Only the edited node loses its record.");
            CollectionAssert.AreEqual(new[] { edited.GUID, other.GUID }, Workspace.GetAITurnSummary(TurnA).NodeIds);
        }

        [Test]
        [Category("UnitTests")]
        public void EditsDuringAnAIEditKeepTheRecord()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            AsAIEdit(() =>
            {
                node.OnNodeModified();
                node.IsFrozen = true;
                node.UpdateValue(new UpdateValueParams("IsVisible", "false"));
            });

            Assert.IsNotNull(node.AIChange);
        }

        [Test]
        [Category("UnitTests")]
        public void RunningTheGraphKeepsTheRecord()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");

            RunCurrentModel();

            Assert.IsNotNull(node.AIChange);
        }

        [Test]
        [Category("UnitTests")]
        public void CustomNodeDefinitionUpdateKeepsTheRecord()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");

            ((HomeWorkspaceModel)Workspace).MarkNodesAsModifiedAndRequestRun(new[] { node }, forceExecute: true);

            Assert.IsNotNull(node.AIChange);
        }

        [Test]
        [Category("UnitTests")]
        public void ChangeFromOutsideTheGraphKeepsTheRecordButStillMarksTheNodeModified()
        {
            // How a File From Path node reports its file changing on disk.
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            var modifiedCount = 0;
            node.Modified += _ => modifiedCount++;

            node.OnNodeModifiedByExternalChange(forceExecute: true);

            Assert.AreEqual(1, modifiedCount, "The node is still reported modified, so it runs again.");
            Assert.IsNotNull(node.AIChange, "Nobody edited the node, so it keeps its record.");
            Assert.IsFalse(node.IsExternalChangeInProgress);

            // A person's edit afterwards still clears it.
            node.OnNodeModified();
            Assert.IsNull(node.AIChange);
        }

        [Test]
        [Category("UnitTests")]
        public void ChangeFromOutsideTheGraphIsSeenOnlyOnTheThreadReportingIt()
        {
            var node = AddNumberNode();
            var seenOnReportingThread = false;
            var seenOnAnotherThread = true;
            Action<NodeModel> listener = n =>
            {
                seenOnReportingThread = n.IsExternalChangeInProgress;
                // Like a second file-watcher callback, or a person's edit on the UI thread, at the same moment.
                seenOnAnotherThread = Task.Run(() => n.IsExternalChangeInProgress).Result;
            };
            node.Modified += listener;

            node.OnNodeModifiedByExternalChange(forceExecute: true);
            node.Modified -= listener;

            Assert.IsTrue(seenOnReportingThread);
            Assert.IsFalse(seenOnAnotherThread, "Another thread must see a person's edit as an edit.");
            Assert.IsFalse(node.IsExternalChangeInProgress);
        }

        [Test]
        [Category("UnitTests")]
        public void ChangeFromOutsideTheGraphReportedInsideAnotherDoesNotEndItEarly()
        {
            var outer = AddNumberNode();
            var inner = AddNumberNode();
            Workspace.RecordAINodeChange(outer.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            Workspace.RecordAINodeChange(inner.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            var outerStillReported = false;
            Action<NodeModel> listener = _ =>
            {
                inner.OnNodeModifiedByExternalChange(forceExecute: true);
                outerStillReported = outer.IsExternalChangeInProgress;
            };
            outer.Modified += listener;

            outer.OnNodeModifiedByExternalChange(forceExecute: true);
            outer.Modified -= listener;

            Assert.IsTrue(outerStillReported);
            Assert.IsNotNull(outer.AIChange);
            Assert.IsNotNull(inner.AIChange);
            Assert.IsFalse(outer.IsExternalChangeInProgress);
            Assert.IsFalse(inner.IsExternalChangeInProgress);
        }

        [Test]
        [Category("UnitTests")]
        public void ChangeFromOutsideTheGraphKeepsTheRecordOfAFrozenNode()
        {
            var node = AddNumberNode();
            AsAIEdit(() => node.IsFrozen = true);
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Frozen", "off", "on");

            node.OnNodeModifiedByExternalChange(forceExecute: true);
            Assert.IsNotNull(node.AIChange);

            node.OnNodeModified();
            Assert.IsNull(node.AIChange, "A person's edit to the frozen node still clears it.");
        }

        [Test]
        [Category("UnitTests")]
        public void PersonFreezingOrEditingAFrozenNodeClearsItsRecord()
        {
            var frozenByPerson = AddNumberNode();
            Workspace.RecordAINodeChange(frozenByPerson.GUID, AIChangeKind.Placed, TurnA);
            frozenByPerson.IsFrozen = true;
            Assert.IsNull(frozenByPerson.AIChange, "Freezing is a person's edit.");

            var editedWhileFrozen = AddNumberNode();
            AsAIEdit(() => editedWhileFrozen.IsFrozen = true);
            Workspace.RecordAINodeChange(editedWhileFrozen.GUID, AIChangeKind.Modified, TurnA, "Frozen", "off", "on");
            editedWhileFrozen.OnNodeModified();
            Assert.IsNull(editedWhileFrozen.AIChange, "Frozen nodes do not raise Modified; the edit still counts.");
        }

        [Test]
        [Category("UnitTests")]
        public void PersonChangingTheAssistantsPreviewChangeClearsTheRecord()
        {
            var node = AddNumberNode();
            AsAIEdit(() => node.UpdateValue(new UpdateValueParams("IsVisible", "false")));
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Preview", "on", "off");
            Assert.IsNotNull(node.AIChange);

            node.UpdateValue(new UpdateValueParams("IsVisible", "true"));

            Assert.IsNull(node.AIChange);
        }

        [Test]
        [Category("UnitTests")]
        public void AssigningTheSameNameKeepsTheRecordButRenamingClearsIt()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            node.Name = node.Name;
            Assert.IsNotNull(node.AIChange, "Re-assigning the same name is not an edit.");

            node.Name = node.Name + " renamed";
            Assert.IsNull(node.AIChange);
        }

        [Test]
        [Category("UnitTests")]
        public void UndoClearsTheNodeRecordButNotTheTurnsHistory()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");

            var undoXml = node.Serialize(new XmlDocument(), SaveContext.Undo);
            StringAssert.DoesNotContain("AIChange", undoXml.OuterXml);
            StringAssert.DoesNotContain(TurnA, undoXml.OuterXml);

            Workspace.ReloadModel(undoXml);

            Assert.IsNull(node.AIChange);
            CollectionAssert.AreEqual(new[] { node.GUID }, Workspace.GetAITurnSummary(TurnA).NodeIds);
        }

        [Test]
        [Category("UnitTests")]
        public void UndoThatMovesAConnectionsStartClearsTheRecordOfTheNodeAtItsEnd()
        {
            // Undoing the deletion of an inline Watch puts a connection's start back this way, without
            // raising Modified on the node at its end.
            var first = AddNumberNode();
            var second = AddNumberNode();
            var end = new WatchNode();
            Workspace.AddAndRegisterNode(end, false);
            var connector = ConnectorModel.Make(first, end, 0, 0);
            Assert.IsNotNull(connector);

            var undoXml = connector.Serialize(new XmlDocument(), SaveContext.Undo);
            Assert.IsTrue(connector.TryUpdateStartPort(second.OutPorts[0], notifyEndNodeModified: false));
            Workspace.RecordAINodeChange(end.GUID, AIChangeKind.Modified, TurnA, "Input", "first", "second");

            Workspace.ReloadModel(undoXml);

            Assert.AreSame(first.OutPorts[0], connector.Start, "Undo puts the connection's start back.");
            Assert.IsNull(end.AIChange);
            CollectionAssert.AreEqual(new[] { end.GUID }, Workspace.GetAITurnSummary(TurnA).NodeIds,
                "The turn's own history is kept.");
        }

        [Test]
        [Category("UnitTests")]
        public void UndoThatLeavesAConnectionsStartAloneKeepsTheRecordOfTheNodeAtItsEnd()
        {
            var start = AddNumberNode();
            var end = new WatchNode();
            Workspace.AddAndRegisterNode(end, false);
            var connector = ConnectorModel.Make(start, end, 0, 0);
            Assert.IsNotNull(connector);
            Workspace.RecordAINodeChange(end.GUID, AIChangeKind.Modified, TurnA, "Input", null, "start");

            Workspace.ReloadModel(connector.Serialize(new XmlDocument(), SaveContext.Undo));

            Assert.AreSame(start.OutPorts[0], connector.Start);
            Assert.IsNotNull(end.AIChange);
        }

        [Test]
        [Category("UnitTests")]
        public void PersonDeletingAnInlineWatchClearsTheRecordOfTheNodeItFed()
        {
            var (before, watch, after) = AddChainThroughInlineWatch();
            Workspace.RecordAINodeChange(after.GUID, AIChangeKind.Modified, TurnA, "Input", null, "Watch");

            Workspace.RecordAndDeleteModels(new List<ModelBase> { watch });

            Assert.AreSame(before.OutPorts[0], after.InPorts[0].Connectors.Single().Start,
                "The node is rewired past the deleted Watch.");
            Assert.IsNull(after.AIChange, "Same as undoing or redoing the deletion.");
            CollectionAssert.AreEqual(new[] { after.GUID }, Workspace.GetAITurnSummary(TurnA).NodeIds,
                "The turn's own history is kept.");
        }

        [Test]
        [Category("UnitTests")]
        public void AIDeletingAnInlineWatchKeepsTheRecordOfTheNodeItFed()
        {
            var (_, watch, after) = AddChainThroughInlineWatch();
            Workspace.RecordAINodeChange(after.GUID, AIChangeKind.Modified, TurnA, "Input", null, "Watch");

            AsAIEdit(() => Workspace.RecordAndDeleteModels(new List<ModelBase> { watch }));

            Assert.IsNotNull(after.AIChange);
        }

        [Test]
        [Category("UnitTests")]
        public void DeletingAnInlineWatchKeepsTheRecordOfTheNodeBeforeIt()
        {
            // The rewire raises IsFrozen on this node, although nobody froze it.
            var (before, watch, _) = AddChainThroughInlineWatch();
            Workspace.RecordAINodeChange(before.GUID, AIChangeKind.Placed, TurnA);

            Workspace.RecordAndDeleteModels(new List<ModelBase> { watch });

            Assert.IsNotNull(before.AIChange);
        }

        [Test]
        [Category("UnitTests")]
        public void RefreshingTheFrozenStateKeepsTheRecordButFreezingStillClearsIt()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            var announced = new List<string>();
            node.PropertyChanged += (_, e) => announced.Add(e.PropertyName);

            // Raises IsFrozen even though nothing was frozen.
            node.ComputeUpstreamOnDownstreamNodes();

            CollectionAssert.Contains(announced, nameof(NodeModel.IsFrozen), "The frozen look is still refreshed.");
            Assert.IsNotNull(node.AIChange, "Nobody froze the node.");
            Assert.IsFalse(node.IsUpstreamCacheRefreshInProgress);

            node.IsFrozen = true;

            Assert.IsNull(node.AIChange, "Freezing is still a person's edit.");
        }

        [Test]
        [Category("UnitTests")]
        public void DefineDataSwitchingBetweenListAndSingleValueWhileRunningKeepsTheRecord()
        {
            // Automatic mode flips IsList itself when the input switches between single value and list.
            var node = AddNode(new DefineDataNode());
            Assert.IsTrue(node.IsAutoMode);
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            var wasList = node.IsList;
            var modifiedCount = 0;
            node.Modified += _ => modifiedCount++;

            ReportRunResultToDefineData(node, isValid: true, updateList: true);

            Assert.AreNotEqual(wasList, node.IsList, "The node switched between list and single value.");
            Assert.AreEqual(1, modifiedCount, "The node is still reported modified, so it runs again.");
            Assert.IsNotNull(node.AIChange, "Nobody edited the node, so it keeps its record.");

            // A person's edit still clears it.
            node.IsList = wasList;
            Assert.IsNull(node.AIChange);
        }

        /// <summary>
        /// Calls DefineData's data bridge callback, as the engine does after a run.
        /// </summary>
        private static void ReportRunResultToDefineData(DefineDataNode node, bool isValid, bool updateList)
        {
            var callback = typeof(DefineDataNode).GetMethod("DataBridgeCallback", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(callback, "DefineData's data bridge callback was not found.");

            callback.Invoke(node, new object[] { (isValid, updateList, (DSCore.Data.DataNodeDynamoType)null) });
        }

        #endregion

        #region AI edit scope

        [Test]
        [Category("UnitTests")]
        public void AIEditScopesNestAndDisposingTwiceIsSafe()
        {
            var outer = Workspace.BeginAIEdit();
            var inner = Workspace.BeginAIEdit();

            inner.Dispose();
            inner.Dispose();
            Assert.IsTrue(Workspace.IsAIEditInProgress);

            outer.Dispose();
            Assert.IsFalse(Workspace.IsAIEditInProgress);
        }

        [Test]
        [Category("UnitTests")]
        public void AIEditEndsWhenTheEditFails()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            Assert.Throws<InvalidOperationException>(() => AsAIEdit(() => throw new InvalidOperationException()));

            Assert.IsFalse(Workspace.IsAIEditInProgress);
            node.OnNodeModified();
            Assert.IsNull(node.AIChange);
        }

        [Test]
        [Category("UnitTests")]
        public void AIEditFlagCannotBeSet()
        {
            var property = typeof(WorkspaceModel).GetProperty(nameof(WorkspaceModel.IsAIEditInProgress));

            Assert.IsFalse(property.CanWrite);
            Assert.IsNull(property.GetSetMethod(nonPublic: true));
        }

        #endregion

        #region Starting a turn and dismissing (DYN-11002)

        [Test]
        [Category("UnitTests")]
        public void StartingATurnClearsEarlierTurnsRecordsButKeepsTheirHistory()
        {
            var placed = AddNumberNode();
            var modified = AddNumberNode();
            Workspace.RecordAINodeChange(placed.GUID, AIChangeKind.Placed, TurnA);
            Workspace.RecordAINodeChange(modified.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");

            Workspace.StartAITurn(TurnB);

            Assert.IsNull(placed.AIChange);
            Assert.IsNull(modified.AIChange);
            CollectionAssert.AreEquivalent(new[] { placed.GUID, modified.GUID }, Workspace.GetAITurnSummary(TurnA).NodeIds,
                "Turn A's own history is kept.");
        }

        [Test]
        [Category("UnitTests")]
        public void StartingATurnKeepsTheRecordsThatTurnAlreadyMade()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            Workspace.StartAITurn(TurnA);

            Assert.AreEqual(TurnA, node.AIChange?.TurnId);
        }

        [Test]
        [Category("UnitTests")]
        public void StartingATurnThatChangesNothingRecordsNoTurn()
        {
            Workspace.StartAITurn(TurnA);

            Assert.IsNull(Workspace.GetAITurnSummary(TurnA));
            CollectionAssert.IsEmpty(Workspace.GetAITurnSummaries());

            // Turn numbers count only turns that changed something
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnB);
            Assert.AreEqual(1, node.AIChange.TurnNumber);
        }

        [Test]
        [Category("UnitTests")]
        public void ChangeInATurnThatWasNotStartedKeepsEarlierRecords()
        {
            // An ungrouped DynamoMCP edit has its own turn and must not clear earlier records
            var earlier = AddNumberNode();
            var later = AddNumberNode();
            Workspace.RecordAINodeChange(earlier.GUID, AIChangeKind.Placed, TurnA);

            Workspace.RecordAINodeChange(later.GUID, AIChangeKind.Placed, "call-1");

            Assert.AreEqual(TurnA, earlier.AIChange?.TurnId);
            Assert.AreEqual("call-1", later.AIChange?.TurnId);
        }

        [Test]
        [Category("UnitTests")]
        public void RecordClearedByANewTurnIsNotBroughtBackWhenThatTurnNetsToNothing()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");

            Workspace.StartAITurn(TurnB);
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnB, "Value", "2", "3");
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnB, "Value", "3", "2");

            Assert.IsNull(node.AIChange, "Turn A's cleared record must not return.");
        }

        [Test]
        [Category("UnitTests")]
        public void StartingATurnWithoutATurnIdIsRejected()
        {
            Assert.Throws<ArgumentException>(() => Workspace.StartAITurn(null));
            Assert.Throws<ArgumentException>(() => Workspace.StartAITurn(string.Empty));
        }

        [Test]
        [Category("UnitTests")]
        public void DismissingClearsEveryRecordButKeepsTheTurnsHistory()
        {
            var first = AddNumberNode();
            var second = AddNumberNode();
            Workspace.RecordAINodeChange(first.GUID, AIChangeKind.Placed, TurnA);
            Workspace.RecordAINodeChange(second.GUID, AIChangeKind.Modified, TurnB, "Value", "1", "2");

            Workspace.DismissAIChanges();

            Assert.IsNull(first.AIChange);
            Assert.IsNull(second.AIChange);
            CollectionAssert.AreEqual(new[] { first.GUID }, Workspace.GetAITurnSummary(TurnA).NodeIds);
            CollectionAssert.AreEqual(new[] { second.GUID }, Workspace.GetAITurnSummary(TurnB).NodeIds);
        }

        [Test]
        [Category("UnitTests")]
        public void TurnInProgressStillReportsEveryChangeAfterADismiss()
        {
            var before = AddNumberNode();
            var after = AddNumberNode();
            Workspace.RecordAINodeChange(before.GUID, AIChangeKind.Placed, TurnA);

            Workspace.DismissAIChanges();
            Workspace.RecordAINodeChange(after.GUID, AIChangeKind.Placed, TurnA);

            Assert.IsNull(before.AIChange);
            Assert.AreEqual(TurnA, after.AIChange?.TurnId);
            CollectionAssert.AreEquivalent(new[] { before.GUID, after.GUID }, Workspace.GetAITurnSummary(TurnA).NodeIds);
        }

        [Test]
        [Category("UnitTests")]
        public void StartingATurnAndDismissingAreNotUndoSteps()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            var couldUndo = Workspace.UndoRecorder.CanUndo;

            Workspace.StartAITurn(TurnB);
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnB, "Value", "1", "2");
            Workspace.DismissAIChanges();

            Assert.AreEqual(couldUndo, Workspace.UndoRecorder.CanUndo);
        }

        #endregion

        #region Never saved, never unsaved

        [Test]
        [Category("UnitTests")]
        public void RecordingAndClearingNeverMarkTheGraphUnsaved()
        {
            var node = AddNumberNode();
            Workspace.HasUnsavedChanges = false;

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            Workspace.RecordAINodeDeleted(node.GUID, TurnA);
            Workspace.RecordAIConnectorRemoved(Guid.NewGuid(), 0, Guid.NewGuid(), 0, TurnA);
            Assert.IsFalse(Workspace.HasUnsavedChanges);

            Workspace.ClearAIChanges();
            Assert.IsFalse(Workspace.HasUnsavedChanges);
        }

        [Test]
        [Category("UnitTests")]
        public void StartingATurnAndDismissingNeverMarkTheGraphUnsaved()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            Workspace.HasUnsavedChanges = false;

            Workspace.StartAITurn(TurnB);
            Assert.IsNull(node.AIChange);
            Assert.IsFalse(Workspace.HasUnsavedChanges);

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnB, "Value", "1", "2");
            Workspace.DismissAIChanges();
            Assert.IsNull(node.AIChange);
            Assert.IsFalse(Workspace.HasUnsavedChanges);
        }

        [Test]
        [Category("UnitTests")]
        public void RecordIsNotWrittenToTheGraphFile()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");

            var json = Workspace.ToJson(CurrentDynamoModel.EngineController);

            StringAssert.DoesNotContain("AIChange", json);
            StringAssert.DoesNotContain("IsRecentlyModifiedByAI", json);
            StringAssert.DoesNotContain("IsAIEditInProgress", json);
            StringAssert.DoesNotContain(TurnA, json);

            Assert.IsNotNull(typeof(NodeModel).GetProperty(nameof(NodeModel.AIChange)).GetCustomAttribute<JsonIgnoreAttribute>());
            Assert.IsNotNull(typeof(NodeModel).GetProperty(nameof(NodeModel.IsRecentlyModifiedByAI)).GetCustomAttribute<JsonIgnoreAttribute>());
            Assert.IsNotNull(typeof(WorkspaceModel).GetProperty(nameof(WorkspaceModel.IsAIEditInProgress)).GetCustomAttribute<JsonIgnoreAttribute>());
        }

        #endregion
    }
}
