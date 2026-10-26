using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// What one AI Assistant turn did to a workspace: the nodes it placed, modified or moved, and how
    /// many nodes and connections it deleted. Read it with <see cref="WorkspaceModel.GetAITurnSummary"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the history of the turn, kept as the turn recorded it. A person editing one of its nodes
    /// afterwards, undo or redo, or a later turn clears or replaces that node's current record
    /// (<see cref="NodeModel.AIChange"/>), but does not change this summary, so its node list and its
    /// counts always describe the same thing.
    /// </para>
    /// <para>
    /// Like the per-node record, it is the net effect of the turn and is never saved to the .dyn file.
    /// A node the turn placed and then deleted again is neither listed nor counted, and a connection the
    /// turn added and then removed again is not counted.
    /// </para>
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

        /// <summary>Identifier of the turn, as reported by the caller that recorded it.</summary>
        public string TurnId { get; }

        /// <summary>Position of the turn among the turns recorded in this workspace, starting at 1.</summary>
        public int TurnNumber { get; }

        /// <summary>
        /// What the turn did to each node it placed, modified or moved, in the order the turn first
        /// touched them. Nodes the turn deleted are counted in <see cref="DeletedNodeCount"/> instead.
        /// </summary>
        public IReadOnlyList<AINodeChange> Nodes { get; }

        /// <summary>The ids of <see cref="Nodes"/>, in the same order.</summary>
        public IReadOnlyList<Guid> NodeIds { get; }

        /// <summary>How many nodes that existed before the turn the turn deleted.</summary>
        public int DeletedNodeCount { get; }

        /// <summary>How many connections that existed before the turn the turn removed.</summary>
        public int RemovedConnectorCount { get; }
    }

    /// <summary>
    /// Everything one turn recorded, kept by <see cref="WorkspaceModel"/>: its own copy of each node's
    /// change, the nodes it placed and deleted, and the connections it added and removed.
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

        /// <summary>This turn's change to a node so far; null when the turn has not changed it.</summary>
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
        /// Whether this turn placed the node. Kept apart from the node's change, so it is still known
        /// after the change is removed, for example when the turn reports deleting the node.
        /// </summary>
        internal bool PlacedNode(Guid nodeId)
        {
            return placedNodes.Contains(nodeId);
        }

        /// <summary>
        /// Remembers the record a node carried when this turn took it over (null for none), so it can be
        /// put back if the turn's net effect on the node turns out to be nothing. Call it whenever the
        /// node's current record is not this turn's: the first time the turn touches the node, and
        /// again when the turn comes back to a node whose record a person cleared, or another turn
        /// replaced, in the meantime. The latest call wins, so a record a person cleared never comes
        /// back.
        /// </summary>
        internal void RememberMarkBeforeTurn(Guid nodeId, AINodeChange mark)
        {
            marksBeforeTurn[nodeId] = mark;
        }

        /// <summary>The record a node carried before this turn touched it, and forgets it.</summary>
        internal AINodeChange TakeMarkBeforeTurn(Guid nodeId)
        {
            marksBeforeTurn.Remove(nodeId, out var mark);
            return mark;
        }

        /// <summary>The turn deleted a node that existed before it. Reporting the same node again counts once.</summary>
        internal void NodeDeleted(Guid nodeId)
        {
            deletedNodes.Add(nodeId);
        }

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
            ReportConnector(key, existsNow: true);
        }

        /// <summary>
        /// The turn removed a connection. Removing one the same turn added cancels that addition, so it
        /// is not counted.
        /// </summary>
        internal void ConnectorRemoved(string key)
        {
            ReportConnector(key, existsNow: false);
        }

        /// <summary>
        /// The first report about a connection tells whether it existed before the turn: one the turn
        /// added did not, one it removed did. Later reports only change whether it exists now, so a
        /// repeated report changes nothing. A connection counts as removed when it existed before the
        /// turn and does not exist now.
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
