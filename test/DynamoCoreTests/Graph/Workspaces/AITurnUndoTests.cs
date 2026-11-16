using System;
using System.Linq;
using CoreNodeModels.Input;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using NUnit.Framework;
using WatchNode = CoreNodeModels.Watch;

namespace Dynamo.Tests
{
    /// <summary>
    /// "Undo all" and "Undo this change" for AI Assistant edits (DYN-11008).
    /// </summary>
    [TestFixture]
    class AITurnUndoTests : DynamoModelTestBase
    {
        private const string TurnA = "turn-a";
        private const string TurnB = "turn-b";

        private WorkspaceModel Workspace => CurrentDynamoModel.CurrentWorkspace;

        #region Helpers

        private void RunManually()
        {
            if (Workspace is HomeWorkspaceModel home)
            {
                home.RunSettings.RunType = RunType.Manual;
            }
        }

        // As DynamoMCP runs an edit
        private void AsTurn(string turn, Action edit)
        {
            using (Workspace.BeginAIEdit(turn))
            {
                edit();
            }
        }

        private T Create<T>(T node, double x = 0, double y = 0) where T : NodeModel
        {
            RunManually();
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(node, x, y, false, false));
            return node;
        }

        private T PlaceByAssistant<T>(string turn, T node, double x = 0, double y = 0) where T : NodeModel
        {
            AsTurn(turn, () =>
            {
                Create(node, x, y);
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, turn);
            });
            return node;
        }

        private void SetValue(NodeModel node, string value)
        {
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(Guid.Empty, node.GUID, "Value", value));
        }

        private void SetValueByAssistant(string turn, NodeModel node, string value)
        {
            AsTurn(turn, () =>
            {
                var before = ValueOf(node);
                SetValue(node, value);
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, turn, "Value", before, value);
            });
        }

        private void Connect(NodeModel start, NodeModel end)
        {
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.MakeConnectionCommand(
                start.GUID, 0, PortType.Output, DynamoModel.MakeConnectionCommand.Mode.Begin));
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.MakeConnectionCommand(
                end.GUID, 0, PortType.Input, DynamoModel.MakeConnectionCommand.Mode.End));
        }

        private void ConnectByAssistant(string turn, NodeModel start, NodeModel end)
        {
            AsTurn(turn, () =>
            {
                Connect(start, end);
                Workspace.RecordAINodeChange(end.GUID, AIChangeKind.Modified, turn, "input", null, start.Name);
            });
        }

        private void Delete(Guid modelId)
        {
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.DeleteModelCommand(modelId));
        }

        private void DeleteByAssistant(string turn, NodeModel node)
        {
            AsTurn(turn, () =>
            {
                Workspace.RecordAINodeDeleted(node.GUID, turn);
                Delete(node.GUID);
            });
        }

        private void Undo()
        {
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Undo));
        }

        private void Redo()
        {
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Redo));
        }

        // Undo and redo recreate deleted nodes as new objects
        private NodeModel Find(Guid id)
        {
            return Workspace.Nodes.FirstOrDefault(n => n.GUID == id);
        }

        private string ValueOf(NodeModel node)
        {
            return ((DoubleInput)Find(node.GUID)).Value;
        }

        private bool IsConnected(NodeModel start, NodeModel end)
        {
            return Workspace.Connectors.Any(c => c.Start.Owner.GUID == start.GUID && c.End.Owner.GUID == end.GUID);
        }

        #endregion

        #region Undo all

        [Test]
        [Category("UnitTests")]
        public void UndoAllRevertsTheWholeTurnAsOneStep()
        {
            var existing = Create(new DoubleInput { Value = "1" });
            var doomed = Create(new DoubleInput());

            var placedId = PlaceByAssistant(TurnA, new DoubleInput(), 200, 0).GUID;
            SetValueByAssistant(TurnA, existing, "2");
            DeleteByAssistant(TurnA, doomed);

            Assert.AreEqual(AIUndoAvailability.Available, Workspace.GetAITurnUndoAvailability(TurnA));
            Assert.IsTrue(Workspace.UndoAITurn(TurnA));

            Assert.IsNull(Find(placedId), "The placed node is removed.");
            Assert.AreEqual("1", ValueOf(existing), "The changed value goes back.");
            Assert.IsNotNull(Find(doomed.GUID), "The deleted node comes back.");
            Assert.IsTrue(Workspace.GetAITurnSummary(TurnA).IsUndone, "The turn is undone, which closes the chip.");

            Redo();

            Assert.IsNotNull(Find(placedId), "One redo brings the whole turn back...");
            Assert.AreEqual("2", ValueOf(existing));
            Assert.IsNull(Find(doomed.GUID));
            Assert.IsNull(Find(placedId).AIChange, "...but not its markers.");
            Assert.IsNull(Find(existing.GUID).AIChange);
            Assert.IsTrue(Workspace.GetAITurnSummary(TurnA).IsUndone, "The chip stays closed after a redo.");
        }

        [Test]
        [Category("UnitTests")]
        public void UndoAllMergesTheTurnsStepsWhenItHadNoUndoGroup()
        {
            var first = PlaceByAssistant(TurnA, new DoubleInput());
            var second = PlaceByAssistant(TurnA, new DoubleInput(), 200, 0);

            Assert.IsTrue(Workspace.UndoAITurn(TurnA));
            Assert.IsNull(Find(first.GUID));
            Assert.IsNull(Find(second.GUID));

            Redo();
            Assert.IsNotNull(Find(first.GUID), "A single redo brings back every step of the turn.");
            Assert.IsNotNull(Find(second.GUID));
        }

        [Test]
        [Category("UnitTests")]
        public void UndoAllIsUnavailableAfterAPersonEdit()
        {
            var placed = PlaceByAssistant(TurnA, new DoubleInput());
            Create(new DoubleInput(), 300, 0);

            Assert.AreEqual(AIUndoAvailability.EditedSince, Workspace.GetAITurnUndoAvailability(TurnA));
            Assert.IsFalse(Workspace.UndoAITurn(TurnA));
            Assert.IsNotNull(Find(placed.GUID), "Nothing is undone.");
        }

        [Test]
        [Category("UnitTests")]
        public void UndoAllIsUnavailableWhenAPersonEditedDuringTheTurn()
        {
            using (Workspace.BeginUndoActionGroup())
            {
                PlaceByAssistant(TurnA, new DoubleInput());
                Create(new DoubleInput(), 300, 0); // A person, between two of the Assistant's edits
                PlaceByAssistant(TurnA, new DoubleInput(), 600, 0);
            }

            Assert.AreEqual(AIUndoAvailability.EditedDuringTurn, Workspace.GetAITurnUndoAvailability(TurnA));
            Assert.IsFalse(Workspace.UndoAITurn(TurnA));
        }

        [Test]
        [Category("UnitTests")]
        public void UndoAllIsUnavailableWhileTheAssistantWorks()
        {
            PlaceByAssistant(TurnA, new DoubleInput());

            using (Workspace.BeginAIEdit(TurnA))
            {
                Assert.AreEqual(AIUndoAvailability.AssistantWorking, Workspace.GetAITurnUndoAvailability(TurnA));
            }

            using (Workspace.BeginUndoActionGroup())
            {
                Assert.AreEqual(AIUndoAvailability.AssistantWorking, Workspace.GetAITurnUndoAvailability(TurnA),
                    "The Assistant's undo group stays open between its tool calls.");
            }

            Assert.AreEqual(AIUndoAvailability.Available, Workspace.GetAITurnUndoAvailability(TurnA));
        }

        [Test]
        [Category("UnitTests")]
        public void UndoAllOnlyUndoesTheLatestTurn()
        {
            var fromA = PlaceByAssistant(TurnA, new DoubleInput());
            Workspace.StartAITurn(TurnB);
            var fromB = PlaceByAssistant(TurnB, new DoubleInput(), 200, 0);

            Assert.AreEqual(AIUndoAvailability.EditedSince, Workspace.GetAITurnUndoAvailability(TurnA),
                "A later turn counts as an edit since.");
            Assert.IsTrue(Workspace.UndoAITurn(TurnB));
            Assert.IsNull(Find(fromB.GUID));
            Assert.IsNotNull(Find(fromA.GUID));
        }

        [Test]
        [Category("UnitTests")]
        public void NothingToUndoForAnUnknownOrAlreadyUndoneTurn()
        {
            Assert.AreEqual(AIUndoAvailability.NothingToUndo, Workspace.GetAITurnUndoAvailability(TurnA));

            PlaceByAssistant(TurnA, new DoubleInput());
            Assert.IsTrue(Workspace.UndoAITurn(TurnA));

            Assert.AreEqual(AIUndoAvailability.NothingToUndo, Workspace.GetAITurnUndoAvailability(TurnA));
            Assert.IsFalse(Workspace.UndoAITurn(TurnA));
        }

        [Test]
        [Category("UnitTests")]
        public void UndoAllIsNotSupportedWhenTheAssistantDoesNotLabelItsEdits()
        {
            // As an older DynamoMCP runs an edit
            var node = new DoubleInput();
            using (Workspace.BeginAIEdit())
            {
                Create(node);
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            }

            Assert.AreEqual(AIUndoAvailability.NotSupported, Workspace.GetAITurnUndoAvailability(TurnA));
            Assert.IsFalse(Workspace.UndoAITurn(TurnA));
            Assert.IsTrue(Workspace.Nodes.Contains(node), "Nothing is undone.");
        }

        #endregion

        #region Undo this change

        [Test]
        [Category("UnitTests")]
        public void UndoThisChangeRemovesANodeTheTurnPlaced()
        {
            var placed = PlaceByAssistant(TurnA, new DoubleInput());

            Assert.AreEqual(AIUndoAvailability.Available, Workspace.GetAINodeUndoAvailability(placed.GUID));
            Assert.IsTrue(Workspace.UndoAINodeChange(placed.GUID));

            Assert.IsNull(Find(placed.GUID));
        }

        [Test]
        [Category("UnitTests")]
        public void UndoThisChangeRestoresTheNodesValueAndPosition()
        {
            var node = Create(new DoubleInput { Value = "1" }, 10, 20);

            AsTurn(TurnA, () =>
            {
                SetValue(node, "5");
                WorkspaceModel.RecordModelForModification(node, Workspace.UndoRecorder);
                node.X = 400;
                node.Y = 300;
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "5");
            });

            Assert.IsTrue(Workspace.UndoAINodeChange(node.GUID));

            Assert.AreEqual("1", ValueOf(node));
            Assert.AreEqual(10, node.X);
            Assert.AreEqual(20, node.Y);
            Assert.IsNull(node.AIChange, "The marker goes with the change.");

            Undo();
            Assert.AreEqual("5", ValueOf(node), "Ctrl+Z brings the change back...");
            Assert.IsNull(Find(node.GUID).AIChange, "...without its marker.");

            Redo();
            Assert.AreEqual("1", ValueOf(node), "Ctrl+Y undoes it again.");
            Assert.AreEqual(10, Find(node.GUID).X);
        }

        [Test]
        [Category("UnitTests")]
        public void UndoThisChangeRestoresTheConnectionsIntoTheNode()
        {
            var original = Create(new DoubleInput());
            var replacement = Create(new DoubleInput(), 0, 200);
            var watch = Create(new WatchNode(), 400, 0);
            Connect(original, watch);
            var originalWire = Workspace.Connectors.Single();

            AsTurn(TurnA, () =>
            {
                Delete(originalWire.GUID);
                Connect(replacement, watch);
                Workspace.RecordAINodeChange(watch.GUID, AIChangeKind.Modified, TurnA, "input", "original", "replacement");
            });
            Assert.IsTrue(IsConnected(replacement, watch));

            Assert.IsTrue(Workspace.UndoAINodeChange(watch.GUID));

            Assert.IsTrue(IsConnected(original, watch), "The removed connection comes back.");
            Assert.IsFalse(IsConnected(replacement, watch), "The added connection goes.");
        }

        [Test]
        [Category("UnitTests")]
        public void NodesThatDependOnTheUndoneChangeKeepTheirMarkers()
        {
            var watch = Create(new WatchNode(), 400, 0);
            var placed = PlaceByAssistant(TurnA, new DoubleInput());
            ConnectByAssistant(TurnA, placed, watch);

            Assert.IsTrue(Workspace.UndoAINodeChange(placed.GUID));

            Assert.IsNull(Find(placed.GUID));
            Assert.AreEqual(0, watch.InPorts[0].Connectors.Count, "Its connection goes with it.");
            Assert.IsNotNull(watch.AIChange, "The node it fed is still the Assistant's change...");
            Assert.AreEqual(AIUndoAvailability.Available, Workspace.GetAINodeUndoAvailability(watch.GUID),
                "...and can still be undone on its own.");
        }

        [Test]
        [Category("UnitTests")]
        public void UndoThisChangeIsUnavailableOnceAPersonEditedTheNode()
        {
            var node = Create(new DoubleInput { Value = "1" });
            SetValueByAssistant(TurnA, node, "5");

            SetValue(node, "7");

            Assert.IsNull(node.AIChange);
            Assert.AreEqual(AIUndoAvailability.NothingToUndo, Workspace.GetAINodeUndoAvailability(node.GUID));
            Assert.IsFalse(Workspace.UndoAINodeChange(node.GUID));
            Assert.AreEqual("7", ValueOf(node));
        }

        [Test]
        [Category("UnitTests")]
        public void UndoThisChangeKeepsThePersonsEditsToOtherNodes()
        {
            var assistants = Create(new DoubleInput { Value = "1" });
            var persons = Create(new DoubleInput { Value = "1" }, 0, 200);
            using (Workspace.BeginUndoActionGroup())
            {
                SetValueByAssistant(TurnA, assistants, "5");
                SetValueByAssistant(TurnA, persons, "5");
            }

            SetValue(persons, "7");
            Assert.AreEqual(AIUndoAvailability.EditedSince, Workspace.GetAITurnUndoAvailability(TurnA));

            Assert.IsTrue(Workspace.UndoAINodeChange(assistants.GUID), "Still available after an edit elsewhere.");

            Assert.AreEqual("1", ValueOf(assistants), "Only the Assistant's change goes back...");
            Assert.AreEqual("7", ValueOf(persons), "...and the person's edit stays.");
            Assert.AreEqual(AIUndoAvailability.EditedSince, Workspace.GetAITurnUndoAvailability(TurnA), "Undo all stays off.");
        }

        [Test]
        [Category("UnitTests")]
        public void UndoThisChangeLeavesItToUndoAllWhenAConnectionCannotComeBack()
        {
            var source = Create(new DoubleInput());
            var replacement = Create(new DoubleInput(), 0, 200);
            var watch = Create(new WatchNode(), 400, 0);
            Connect(source, watch);

            // The turn deletes the source and rewires the target
            AsTurn(TurnA, () =>
            {
                Workspace.RecordAINodeDeleted(source.GUID, TurnA);
                Delete(source.GUID);
                Connect(replacement, watch);
                Workspace.RecordAINodeChange(watch.GUID, AIChangeKind.Modified, TurnA, "input", "source", "replacement");
            });

            Assert.AreEqual(AIUndoAvailability.DependsOnOtherChanges, Workspace.GetAINodeUndoAvailability(watch.GUID));
            Assert.IsFalse(Workspace.UndoAINodeChange(watch.GUID));
            Assert.IsTrue(IsConnected(replacement, watch), "Nothing changes: the new connection stays...");
            Assert.IsNotNull(watch.AIChange, "...and so does the marker.");

            Assert.IsTrue(Workspace.UndoAITurn(TurnA), "Undo all restores the whole turn...");
            Assert.IsNotNull(Find(source.GUID));
            Assert.IsTrue(IsConnected(source, watch), "...with the original connection.");
            Assert.IsFalse(IsConnected(replacement, watch));
        }

        [Test]
        [Category("UnitTests")]
        public void UndoThisChangeIsUnavailableWhileTheAssistantWorks()
        {
            var placed = PlaceByAssistant(TurnA, new DoubleInput());

            using (Workspace.BeginAIEdit(TurnA))
            {
                Assert.AreEqual(AIUndoAvailability.AssistantWorking, Workspace.GetAINodeUndoAvailability(placed.GUID));
                Assert.IsFalse(Workspace.UndoAINodeChange(placed.GUID));
            }

            Assert.IsNotNull(Find(placed.GUID));
        }

        [Test]
        [Category("UnitTests")]
        public void UndoAllStillWorksAfterUndoThisChange()
        {
            var first = Create(new DoubleInput { Value = "1" });
            var second = Create(new DoubleInput { Value = "1" }, 0, 200);
            using (Workspace.BeginUndoActionGroup())
            {
                SetValueByAssistant(TurnA, first, "5");
                SetValueByAssistant(TurnA, second, "5");
            }

            Assert.IsTrue(Workspace.UndoAINodeChange(first.GUID));
            Assert.AreEqual("1", ValueOf(first));

            Assert.AreEqual(AIUndoAvailability.Available, Workspace.GetAITurnUndoAvailability(TurnA),
                "Undo this change does not count as an edit since the turn.");
            Assert.IsTrue(Workspace.UndoAITurn(TurnA));
            Assert.AreEqual("1", ValueOf(first));
            Assert.AreEqual("1", ValueOf(second));

            Redo();
            Assert.AreEqual("1", ValueOf(first), "Redo brings back the turn and the undone change together.");
            Assert.AreEqual("5", ValueOf(second));
        }

        #endregion
    }
}
