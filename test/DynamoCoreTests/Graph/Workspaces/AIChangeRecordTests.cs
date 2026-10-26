using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using CoreNodeModels.Input;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Newtonsoft.Json;
using NUnit.Framework;

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
            if (Workspace is HomeWorkspaceModel home)
            {
                home.RunSettings.RunType = RunType.Manual;
            }

            var node = new DoubleInput();
            Workspace.AddAndRegisterNode(node, false);
            return node;
        }

        /// <summary>Runs an edit as DynamoMCP does: with IsAIEditInProgress set.</summary>
        private void AsAIEdit(Action edit)
        {
            Workspace.IsAIEditInProgress = true;
            try
            {
                edit();
            }
            finally
            {
                Workspace.IsAIEditInProgress = false;
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
