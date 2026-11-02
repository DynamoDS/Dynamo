using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// What one AI Assistant turn did to a workspace: the nodes it placed, modified or moved, and how
    /// many nodes and connections it deleted. See <see cref="WorkspaceModel.GetAITurnSummary"/>.
    /// </summary>
    /// <remarks>
    /// The turn's own history. A later edit, undo/redo or another turn changes a node's current record
    /// (<see cref="NodeModel.AIChange"/>), not this summary. Holds the turn's net effect: a node it
    /// placed then deleted, or a connection it added then removed, is not counted. Never saved to the
    /// .dyn file.
    /// </remarks>
    public sealed class AITurnSummary
    {
        internal AITurnSummary(
            string turnId,
            int turnNumber,
            IReadOnlyList<AINodeChange> nodes,
            int deletedNodeCount,
            int removedConnectorCount)
        {
            TurnId = turnId;
            TurnNumber = turnNumber;
            Nodes = nodes;
            NodeIds = nodes.Select(n => n.NodeId).ToList().AsReadOnly();
            DeletedNodeCount = deletedNodeCount;
            RemovedConnectorCount = removedConnectorCount;
        }

        /// <summary>The turn's identifier, as given by the caller.</summary>
        public string TurnId { get; }

        /// <summary>The turn's 1-based position in this workspace.</summary>
        public int TurnNumber { get; }

        /// <summary>
        /// Each node the turn placed, modified or moved, in the order first touched. Deleted nodes are in
        /// <see cref="DeletedNodeCount"/> instead.
        /// </summary>
        public IReadOnlyList<AINodeChange> Nodes { get; }

        /// <summary>The ids of <see cref="Nodes"/>, in the same order.</summary>
        public IReadOnlyList<Guid> NodeIds { get; }

        /// <summary>How many pre-existing nodes the turn deleted.</summary>
        public int DeletedNodeCount { get; }

        /// <summary>How many pre-existing connections the turn removed.</summary>
        public int RemovedConnectorCount { get; }
    }

    /// <summary>
    /// Everything one turn recorded: its copy of each node's change, the nodes it placed and deleted,
    /// and the connections it added and removed.
    /// </summary>
    internal sealed class AITurnState
    {
        private readonly List<Guid> nodeOrder = new List<Guid>();
        private readonly Dictionary<Guid, AINodeChange> nodeChanges = new Dictionary<Guid, AINodeChange>();
        private readonly HashSet<Guid> placedNodes = new HashSet<Guid>();
        private readonly HashSet<Guid> deletedNodes = new HashSet<Guid>();
        private readonly Dictionary<Guid, AINodeChange> marksBeforeTurn = new Dictionary<Guid, AINodeChange>();
        private readonly Dictionary<string, ConnectorPresence> connectors = new Dictionary<string, ConnectorPresence>(StringComparer.Ordinal);

        internal AITurnState(string turnId, int turnNumber)
        {
            TurnId = turnId;
            TurnNumber = turnNumber;
        }

        internal string TurnId { get; }

        internal int TurnNumber { get; }

        internal int DeletedNodeCount => deletedNodes.Count;

        internal int RemovedConnectorCount => connectors.Values.Count(c => c.ExistedBeforeTurn && !c.ExistsNow);

        /// <summary>The turn's changes to each node, in the order the turn first touched them.</summary>
        internal IReadOnlyList<AINodeChange> NodeChanges => nodeOrder.Select(id => nodeChanges[id]).ToList().AsReadOnly();

        /// <summary>This turn's change to a node so far, or null.</summary>
        internal AINodeChange GetNodeChange(Guid nodeId)
        {
            return nodeChanges.TryGetValue(nodeId, out var change) ? change : null;
        }

        /// <summary>Stores this turn's change to a node; null removes the node from the turn.</summary>
        internal void SetNodeChange(Guid nodeId, AINodeChange change)
        {
            if (change == null)
            {
                if (nodeChanges.Remove(nodeId))
                {
                    nodeOrder.Remove(nodeId);
                }

                return;
            }

            if (!nodeChanges.ContainsKey(nodeId))
            {
                nodeOrder.Add(nodeId);
            }

            nodeChanges[nodeId] = change;
            if (change.Kind == AIChangeKind.Placed)
            {
                placedNodes.Add(nodeId);
            }
        }

        /// <summary>
        /// Whether this turn placed the node. Kept separately, so it survives the node's change being
        /// removed (for example when the turn deletes the node).
        /// </summary>
        internal bool PlacedNode(Guid nodeId)
        {
            return placedNodes.Contains(nodeId);
        }

        /// <summary>
        /// Remembers the record a node had when this turn took it over (null for none), to restore if the
        /// turn's net effect is nothing. Call whenever the node's record is not this turn's, including
        /// after a person cleared it or another turn replaced it. The latest call wins, so a cleared
        /// record never comes back.
        /// </summary>
        internal void RememberMarkBeforeTurn(Guid nodeId, AINodeChange mark)
        {
            marksBeforeTurn[nodeId] = mark;
        }

        /// <summary>Returns and forgets the record a node had before this turn.</summary>
        internal AINodeChange TakeMarkBeforeTurn(Guid nodeId)
        {
            marksBeforeTurn.Remove(nodeId, out var mark);
            return mark;
        }

        /// <summary>The turn deleted a pre-existing node. Repeat reports count once.</summary>
        internal void NodeDeleted(Guid nodeId)
        {
            deletedNodes.Add(nodeId);
        }

        /// <summary>Identifies a connection by the ports at its two ends.</summary>
        internal static string ConnectorKey(Guid startNodeId, int startPortIndex, Guid endNodeId, int endPortIndex)
        {
            return $"{startNodeId:N}:{startPortIndex}>{endNodeId:N}:{endPortIndex}";
        }

        /// <summary>The turn added a connection. Re-adding one it removed cancels the removal.</summary>
        internal void ConnectorAdded(string key)
        {
            ReportConnector(key, existsNow: true);
        }

        /// <summary>The turn removed a connection. Removing one it added cancels the addition.</summary>
        internal void ConnectorRemoved(string key)
        {
            ReportConnector(key, existsNow: false);
        }

        /// <summary>
        /// The first report tells whether the connection existed before the turn (removed: yes, added:
        /// no). Later reports only update whether it exists now. It counts as removed if it existed
        /// before and does not now.
        /// </summary>
        private void ReportConnector(string key, bool existsNow)
        {
            var existedBeforeTurn = connectors.TryGetValue(key, out var known)
                ? known.ExistedBeforeTurn
                : !existsNow;
            connectors[key] = new ConnectorPresence(existedBeforeTurn, existsNow);
        }

        /// <summary>Whether a connection existed before the turn, and whether it exists now.</summary>
        private readonly struct ConnectorPresence
        {
            internal ConnectorPresence(bool existedBeforeTurn, bool existsNow)
            {
                ExistedBeforeTurn = existedBeforeTurn;
                ExistsNow = existsNow;
            }

            internal bool ExistedBeforeTurn { get; }

            internal bool ExistsNow { get; }
        }
    }
}
