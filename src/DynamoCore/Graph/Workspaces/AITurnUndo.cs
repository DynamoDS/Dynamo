using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;
using Dynamo.Core;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// Whether an AI Assistant change can be undone from the review UI (DYN-11008), and if not, why.
    /// </summary>
    internal enum AIUndoAvailability
    {
        Available,
        NothingToUndo,
        AssistantWorking,
        EditedSince,
        EditedDuringTurn,
        NotSupported,
        DependsOnOtherChanges,
    }

    public partial class WorkspaceModel
    {
        #region AI Assistant turn undo (DYN-11008)

        private sealed class AIUndoSnapshot
        {
            internal AIUndoSnapshot(UndoRedoRecorder.UserAction firstAction, XmlElement state, bool isConnector)
            {
                FirstAction = firstAction;
                State = state;
                IsConnector = isConnector;
            }

            internal UndoRedoRecorder.UserAction FirstAction { get; }

            internal XmlElement State { get; }

            internal bool IsConnector { get; }

            internal bool CreatedByTurn => FirstAction == UndoRedoRecorder.UserAction.Creation;

            internal Guid EndNodeId => IsConnector ? new XmlElementHelper(State).ReadGuid("end", Guid.Empty) : Guid.Empty;
        }

        private readonly Dictionary<string, Dictionary<Guid, AIUndoSnapshot>> aiUndoSnapshots =
            new Dictionary<string, Dictionary<Guid, AIUndoSnapshot>>(StringComparer.Ordinal);

        private string reviewUndoTurnId;

        private bool IsAssistantWorking => IsAIEditInProgress || IsUndoActionGroupOpen;

        private string CurrentAITurnForUndo()
        {
            if (reviewUndoTurnId != null)
            {
                return reviewUndoTurnId;
            }

            if (!IsAIEditInProgress)
            {
                return null;
            }

            return Volatile.Read(ref currentAITurnId) ?? UndoRedoRecorder.UnknownAITurn;
        }

        private void RememberAIUndoSnapshot(ModelBase model, UndoRedoRecorder.UserAction action, XmlElement state)
        {
            if (reviewUndoTurnId != null || !IsAIEditInProgress)
            {
                return;
            }

            var turnId = Volatile.Read(ref currentAITurnId);
            if (turnId == null)
            {
                return;
            }

            bool isConnector;
            Guid modelId;
            if (model is NodeModel node)
            {
                isConnector = false;
                modelId = node.GUID;
            }
            else if (model is ConnectorModel connector)
            {
                isConnector = true;
                modelId = connector.GUID;
            }
            else if (model == null && state.HasAttribute("end"))
            {
                // A connector that a node modification removed
                isConnector = true;
                modelId = new XmlElementHelper(state).ReadGuid("guid", Guid.Empty);
                if (modelId == Guid.Empty)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            // Own copy: the recorder may move its element into another undo step
            var snapshot = new AIUndoSnapshot(action, (XmlElement)state.CloneNode(true), isConnector);
            lock (aiTurnsLock)
            {
                if (!aiUndoSnapshots.TryGetValue(turnId, out var snapshots))
                {
                    snapshots = new Dictionary<Guid, AIUndoSnapshot>();
                    aiUndoSnapshots.Add(turnId, snapshots);
                }

                // The first recording is the state before the turn
                snapshots.TryAdd(modelId, snapshot);
            }
        }

        private void ForgetAIUndoSnapshotsExcept(string turnId)
        {
            foreach (var other in aiUndoSnapshots.Keys.Where(id => !string.Equals(id, turnId, StringComparison.Ordinal)).ToList())
            {
                aiUndoSnapshots.Remove(other);
            }
        }

        /// <summary>
        /// Undo all is available while the turn's undo steps are the newest ones and hold only the turn's edits.
        /// </summary>
        internal AIUndoAvailability GetAITurnUndoAvailability(string turnId)
        {
            if (string.IsNullOrEmpty(turnId))
            {
                return AIUndoAvailability.NothingToUndo;
            }

            if (IsAssistantWorking)
            {
                return AIUndoAvailability.AssistantWorking;
            }

            return TurnUndoAvailability(undoRecorder.GetUndoStackOrigins(), turnId, out _);
        }

        private static AIUndoAvailability TurnUndoAvailability(
            IReadOnlyList<(string AITurn, bool HasOtherEdits)> origins,
            string turnId,
            out int steps)
        {
            bool IsTurnStep((string AITurn, bool HasOtherEdits) origin) =>
                string.Equals(origin.AITurn, turnId, StringComparison.Ordinal);

            // The turn's own steps at the top of the stack
            steps = 0;
            while (steps < origins.Count && IsTurnStep(origins[steps]) && !origins[steps].HasOtherEdits)
            {
                steps++;
            }

            var turnStepsBelow = origins.Skip(steps).Where(IsTurnStep).ToList();
            if (turnStepsBelow.Any(origin => origin.HasOtherEdits))
            {
                return AIUndoAvailability.EditedDuringTurn;
            }

            if (turnStepsBelow.Count > 0)
            {
                return AIUndoAvailability.EditedSince;
            }

            if (steps > 0)
            {
                return AIUndoAvailability.Available;
            }

            return origins.Any(origin => origin.AITurn == UndoRedoRecorder.UnknownAITurn)
                ? AIUndoAvailability.NotSupported
                : AIUndoAvailability.NothingToUndo;
        }

        /// <summary>
        /// Undo all: reverts the whole turn as one undo step and closes the summary chip. Redo brings the
        /// changes back without their markers.
        /// </summary>
        internal bool UndoAITurn(string turnId)
        {
            if (GetAITurnUndoAvailability(turnId) != AIUndoAvailability.Available)
            {
                return false;
            }

            TurnUndoAvailability(undoRecorder.GetUndoStackOrigins(), turnId, out var steps);
            undoRecorder.MergeTopActionGroups(steps);
            var stepsBeforeUndo = undoRecorder.GetUndoStackOrigins().Count;
            Undo();

            // Undo did nothing, so the turn is not undone and keeps its markers
            if (undoRecorder.GetUndoStackOrigins().Count >= stepsBeforeUndo)
            {
                return false;
            }

            // Undo clears the markers of the nodes it reloads; clear the rest
            foreach (var node in Nodes.Where(n => string.Equals(n.AIChange?.TurnId, turnId, StringComparison.Ordinal)).ToList())
            {
                node.ClearAIChange();
            }

            lock (aiTurnsLock)
            {
                if (aiTurns.TryGetValue(turnId, out var turn))
                {
                    turn.IsUndone = true;
                }

                aiUndoSnapshots.Remove(turnId);
            }

            OnAIChangesUpdated();
            return true;
        }

        /// <summary>
        /// "Undo this change" is available while the node still carries the turn's marker, and every
        /// connection the turn removed from it can come back (the node it came from still has that output).
        /// </summary>
        internal AIUndoAvailability GetAINodeUndoAvailability(Guid nodeId)
        {
            if (IsAssistantWorking)
            {
                return AIUndoAvailability.AssistantWorking;
            }

            var change = FindNodeForAIRecord(nodeId)?.AIChange;
            if (change == null)
            {
                return AIUndoAvailability.NothingToUndo;
            }

            List<AIUndoSnapshot> removedConnectors;
            lock (aiTurnsLock)
            {
                if (!aiUndoSnapshots.TryGetValue(change.TurnId, out var snapshots) || !HasAIUndoSnapshotFor(snapshots, nodeId))
                {
                    return AIUndoAvailability.NothingToUndo;
                }

                removedConnectors = snapshots.Values
                    .Where(snapshot => snapshot.IsConnector && !snapshot.CreatedByTurn && snapshot.EndNodeId == nodeId)
                    .ToList();
            }

            // Checked before anything changes, so a refused undo leaves the graph and the marker alone
            return removedConnectors.All(snapshot => FindAIConnectorStart(snapshot.State) != null)
                ? AIUndoAvailability.Available
                : AIUndoAvailability.DependsOnOtherChanges;
        }

        private static bool HasAIUndoSnapshotFor(Dictionary<Guid, AIUndoSnapshot> snapshots, Guid nodeId)
        {
            return snapshots.ContainsKey(nodeId)
                || snapshots.Values.Any(snapshot => snapshot.IsConnector && snapshot.EndNodeId == nodeId);
        }

        /// <summary>
        /// "Undo this change": reverts what the turn did to one node, as a new undo step of that turn.
        /// A placed node is removed; otherwise the node and its input connections go back to their state
        /// before the turn. Other nodes keep their markers.
        /// </summary>
        internal bool UndoAINodeChange(Guid nodeId)
        {
            if (GetAINodeUndoAvailability(nodeId) != AIUndoAvailability.Available)
            {
                return false;
            }

            var node = FindNodeForAIRecord(nodeId);
            var turnId = node.AIChange.TurnId;
            Dictionary<Guid, AIUndoSnapshot> snapshots;
            lock (aiTurnsLock)
            {
                snapshots = new Dictionary<Guid, AIUndoSnapshot>(aiUndoSnapshots[turnId]);
            }

            snapshots.TryGetValue(nodeId, out var nodeSnapshot);

            using (BeginDelayedGraphExecution())
            // An AI edit, so the nodes this one feeds keep their markers
            using (BeginAIEdit())
            {
                reviewUndoTurnId = turnId;
                try
                {
                    using (undoRecorder.BeginActionGroup())
                    {
                        if (nodeSnapshot?.CreatedByTurn == true)
                        {
                            RecordAndDeleteModels(new List<ModelBase> { node });
                        }
                        else
                        {
                            RestoreAINodeChange(node, nodeSnapshot, snapshots);
                        }
                    }
                }
                finally
                {
                    reviewUndoTurnId = null;
                }
            }

            // Not reloaded when only its connections changed
            node.ClearAIChange();

            lock (aiTurnsLock)
            {
                if (aiUndoSnapshots.TryGetValue(turnId, out var turnSnapshots))
                {
                    turnSnapshots.Remove(nodeId);
                }
            }

            OnAIChangesUpdated();
            return true;
        }

        // Removes the connections the turn added into the node, restores the node, then restores the
        // connections into it that the turn removed or moved
        private void RestoreAINodeChange(NodeModel node, AIUndoSnapshot nodeSnapshot, Dictionary<Guid, AIUndoSnapshot> snapshots)
        {
            var addedConnectors = node.InPorts
                .SelectMany(port => port.Connectors)
                .Where(connector => snapshots.TryGetValue(connector.GUID, out var snapshot) && snapshot.CreatedByTurn)
                .Distinct()
                .Cast<ModelBase>()
                .ToList();
            if (addedConnectors.Count > 0)
            {
                RecordAndDeleteModels(addedConnectors);
            }

            if (nodeSnapshot != null)
            {
                undoRecorder.RecordModificationForUndo(node);
                ReloadModel(nodeSnapshot.State);
            }

            foreach (var pair in snapshots)
            {
                var snapshot = pair.Value;
                if (!snapshot.IsConnector || snapshot.CreatedByTurn || snapshot.EndNodeId != node.GUID)
                {
                    continue;
                }

                if (GetModelInternal(pair.Key) is ConnectorModel existing)
                {
                    // Still there, so the turn moved one of its ends
                    undoRecorder.RecordModificationForUndo(existing);
                    ReloadModel(snapshot.State);
                }
                else if (CanReconnectAIConnector(snapshot.State, node))
                {
                    CreateModel(snapshot.State);
                    if (GetModelInternal(pair.Key) is ConnectorModel restored)
                    {
                        undoRecorder.RecordCreationForUndo(restored);
                    }
                }
            }
        }

        private bool CanReconnectAIConnector(XmlElement connectorState, NodeModel endNode)
        {
            var endIndex = new XmlElementHelper(connectorState).ReadInteger("end_index", -1);

            return FindAIConnectorStart(connectorState) != null
                && endIndex >= 0 && endIndex < endNode.InPorts.Count
                && endNode.InPorts[endIndex].Connectors.Count == 0;
        }

        // The node a connection came from, if it is still there with that output
        private NodeModel FindAIConnectorStart(XmlElement connectorState)
        {
            var helper = new XmlElementHelper(connectorState);
            var startNode = GetModelInternal(helper.ReadGuid("start", Guid.Empty)) as NodeModel;
            var startIndex = helper.ReadInteger("start_index", -1);

            return startNode != null && startIndex >= 0 && startIndex < startNode.OutPorts.Count ? startNode : null;
        }

        #endregion
    }
}
