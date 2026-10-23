using System;
using System.Collections.Generic;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// What one AI Assistant turn did to a workspace: the nodes it placed, modified or moved, and how
    /// many nodes and connections it deleted. Read it with <see cref="WorkspaceModel.GetAITurnSummary"/>.
    /// </summary>
    /// <remarks>
    /// Like the per-node record, this is the net effect of the turn and is never saved to the .dyn file.
    /// A node the turn placed and then deleted again is neither listed nor counted, and a connection the
    /// turn added and then removed again is not counted.
    /// </remarks>
    public sealed class AITurnSummary
    {
        internal AITurnSummary(
            string turnId,
            int turnNumber,
            IReadOnlyList<Guid> nodeIds,
            int deletedNodeCount,
            int removedConnectorCount)
        {
            TurnId = turnId;
            TurnNumber = turnNumber;
            NodeIds = nodeIds;
            DeletedNodeCount = deletedNodeCount;
            RemovedConnectorCount = removedConnectorCount;
        }

        /// <summary>Identifier of the turn, as reported by the caller that recorded it.</summary>
        public string TurnId { get; }

        /// <summary>Position of the turn among the turns recorded in this workspace, starting at 1.</summary>
        public int TurnNumber { get; }

        /// <summary>
        /// The nodes whose current record (<see cref="Dynamo.Graph.Nodes.NodeModel.AIChange"/>) belongs to
        /// this turn. A node drops out of the list when a person edits it, when undo or redo changes it,
        /// or when a later turn changes it.
        /// </summary>
        public IReadOnlyList<Guid> NodeIds { get; }

        /// <summary>How many nodes that existed before the turn the turn deleted.</summary>
        public int DeletedNodeCount { get; }

        /// <summary>How many connections that existed before the turn the turn removed.</summary>
        public int RemovedConnectorCount { get; }
    }

    /// <summary>
    /// Running totals for one turn, kept by <see cref="WorkspaceModel"/> while the turn is recorded.
    /// </summary>
    internal sealed class AITurnState
    {
        private readonly HashSet<string> addedConnectors = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> removedConnectors = new HashSet<string>(StringComparer.Ordinal);

        internal AITurnState(string turnId, int turnNumber)
        {
            TurnId = turnId;
            TurnNumber = turnNumber;
        }

        internal string TurnId { get; }

        internal int TurnNumber { get; }

        internal int DeletedNodeCount { get; set; }

        internal int RemovedConnectorCount => removedConnectors.Count;

        /// <summary>Identifies a connection by the ports at its two ends.</summary>
        internal static string ConnectorKey(Guid startNodeId, int startPortIndex, Guid endNodeId, int endPortIndex)
        {
            return $"{startNodeId:N}:{startPortIndex}>{endNodeId:N}:{endPortIndex}";
        }

        /// <summary>
        /// The turn added a connection. Re-adding one the same turn removed cancels that removal.
        /// </summary>
        internal void ConnectorAdded(string key)
        {
            if (!removedConnectors.Remove(key))
            {
                addedConnectors.Add(key);
            }
        }

        /// <summary>
        /// The turn removed a connection. Removing one the same turn added cancels that addition, so it
        /// is not counted.
        /// </summary>
        internal void ConnectorRemoved(string key)
        {
            if (!addedConnectors.Remove(key))
            {
                removedConnectors.Add(key);
            }
        }
    }
}
