using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Graph.Nodes
{
    /// <summary>
    /// How an AI Assistant turn changed a node. If a turn changes a node in several ways, the highest
    /// value wins: <see cref="Placed"/>, then <see cref="Modified"/>, then <see cref="Moved"/>.
    /// </summary>
    public enum AIChangeKind
    {
        /// <summary>Moved on the canvas; no input changed.</summary>
        Moved = 0,

        /// <summary>One or more inputs changed. See <see cref="AINodeChange.InputChanges"/>.</summary>
        Modified = 1,

        /// <summary>Added to the graph.</summary>
        Placed = 2,
    }

    /// <summary>
    /// A node input an AI Assistant turn changed, with its value before and after the turn. An input is
    /// a port's wiring or a setting that feeds the result (slider value, Code Block text, Python script
    /// or engine, name, lacing).
    /// </summary>
    public sealed class AIInputChange
    {
        internal AIInputChange(string inputName, string before, string after)
        {
            InputName = inputName;
            Before = before;
            After = after;
        }

        /// <summary>
        /// A port name for wiring, or "Value", "Code", "Script", "Engine", "Name" or "Lacing" for a setting.
        /// </summary>
        public string InputName { get; }

        /// <summary>The value before the turn, as display text. Null when there was none.</summary>
        public string Before { get; }

        /// <summary>The value after the turn, as display text. Null when there is none.</summary>
        public string After { get; }
    }

    /// <summary>
    /// What one AI Assistant turn did to one node. The node's current record is
    /// <see cref="NodeModel.AIChange"/>; the turn's own copy is in
    /// <see cref="Dynamo.Graph.Workspaces.AITurnSummary.Nodes"/>.
    /// </summary>
    /// <remarks>
    /// Holds the turn's net effect: a value changed back, or a node moved back, leaves no record. Never
    /// saved to the .dyn file; never marks the workspace as unsaved.
    /// </remarks>
    public sealed class AINodeChange
    {
        private static readonly IReadOnlyList<AIInputChange> NoInputs = Array.Empty<AIInputChange>();

        private AINodeChange(
            Guid nodeId,
            AIChangeKind kind,
            string turnId,
            int turnNumber,
            IReadOnlyList<AIInputChange> inputChanges,
            AIMove move)
        {
            NodeId = nodeId;
            Kind = kind;
            TurnId = turnId;
            TurnNumber = turnNumber;
            InputChanges = inputChanges;
            Move = move;
        }

        /// <summary>The node the turn changed.</summary>
        public Guid NodeId { get; }

        /// <summary>How the turn changed the node.</summary>
        public AIChangeKind Kind { get; }

        /// <summary>The turn's identifier, as given by the caller.</summary>
        public string TurnId { get; }

        /// <summary>
        /// The turn's 1-based position in this workspace, for display ("Turn 3"). Use
        /// <see cref="TurnId"/> to identify a turn.
        /// </summary>
        public int TurnNumber { get; }

        /// <summary>
        /// Changed inputs, in the order first changed. Empty for <see cref="AIChangeKind.Placed"/> and
        /// <see cref="AIChangeKind.Moved"/>.
        /// </summary>
        public IReadOnlyList<AIInputChange> InputChanges { get; }

        /// <summary>Where the turn moved the node from and to; empty if not moved.</summary>
        internal AIMove Move { get; }

        /// <summary>
        /// Adds one change to a node's record. Returns the new record, or null if the turn's net effect
        /// on the node is nothing.
        /// </summary>
        /// <param name="current">The node's record so far in this turn, or null.</param>
        /// <param name="nodeId">The node that changed.</param>
        /// <param name="turnId">Turn that made the change.</param>
        /// <param name="turnNumber">Display number of that turn.</param>
        /// <param name="report">What the change was.</param>
        internal static AINodeChange Apply(
            AINodeChange current,
            Guid nodeId,
            string turnId,
            int turnNumber,
            AIChangeReport report)
        {
            // A record describes one turn, so a new turn starts fresh
            if (current != null && !string.Equals(current.TurnId, turnId, StringComparison.Ordinal))
            {
                current = null;
            }

            // A placed node is all new: no before/after to keep
            if (report.Kind == AIChangeKind.Placed || current?.Kind == AIChangeKind.Placed)
            {
                return new AINodeChange(nodeId, AIChangeKind.Placed, turnId, turnNumber, NoInputs, AIMove.None);
            }

            var inputs = current?.InputChanges.ToList() ?? new List<AIInputChange>();
            var move = current?.Move ?? AIMove.None;

            if (report.Kind == AIChangeKind.Moved)
            {
                move = move.Then(report.Before, report.After);
            }
            else
            {
                ApplyInputChange(inputs, report);
            }

            if (inputs.Count > 0)
            {
                return new AINodeChange(nodeId, AIChangeKind.Modified, turnId, turnNumber, inputs.AsReadOnly(), move);
            }

            return move.IsNetMove
                ? new AINodeChange(nodeId, AIChangeKind.Moved, turnId, turnNumber, NoInputs, move)
                : null;
        }

        /// <summary>
        /// Adds a changed input, keeping its value before the turn and its latest value. An input changed
        /// back to its value before the turn is removed.
        /// </summary>
        private static void ApplyInputChange(List<AIInputChange> inputs, AIChangeReport report)
        {
            if (string.IsNullOrEmpty(report.InputName))
            {
                throw new ArgumentException("A modified node must name the input that changed.", nameof(report));
            }

            var index = inputs.FindIndex(i => string.Equals(i.InputName, report.InputName, StringComparison.Ordinal));
            var turnBefore = index >= 0 ? inputs[index].Before : report.Before;
            if (index >= 0)
            {
                inputs.RemoveAt(index);
            }
            else
            {
                index = inputs.Count;
            }

            if (!string.Equals(turnBefore, report.After, StringComparison.Ordinal))
            {
                inputs.Insert(index, new AIInputChange(report.InputName, turnBefore, report.After));
            }
        }
    }

    /// <summary>
    /// One change passed to <see cref="AINodeChange.Apply"/>: its kind and, for an input or a move, the
    /// value or position before and after.
    /// </summary>
    internal readonly struct AIChangeReport
    {
        internal AIChangeReport(AIChangeKind kind, string inputName, string before, string after)
        {
            Kind = kind;
            InputName = inputName;
            Before = before;
            After = after;
        }

        internal AIChangeKind Kind { get; }

        /// <summary>For <see cref="AIChangeKind.Modified"/>: the input that changed. Required.</summary>
        internal string InputName { get; }

        /// <summary>The input's value, or for a move the position, before the change.</summary>
        internal string Before { get; }

        /// <summary>The input's value, or for a move the position, after the change.</summary>
        internal string After { get; }
    }

    /// <summary>
    /// Where a turn moved a node: before its first move and after its last.
    /// </summary>
    internal readonly struct AIMove
    {
        /// <summary>The turn did not move the node.</summary>
        internal static readonly AIMove None = default;

        private AIMove(string before, string after)
        {
            HasMove = true;
            Before = before;
            After = after;
        }

        /// <summary>True if the turn moved the node at least once.</summary>
        internal bool HasMove { get; }

        /// <summary>Position before the turn's first move, as reported.</summary>
        internal string Before { get; }

        /// <summary>Position after the turn's last move, as reported.</summary>
        internal string After { get; }

        /// <summary>
        /// False if the node moved back to where it started. A move without reported positions always
        /// counts.
        /// </summary>
        internal bool IsNetMove =>
            HasMove && !(Before != null && string.Equals(Before, After, StringComparison.Ordinal));

        /// <summary>Adds one more move: keeps where the node started the turn, follows where it ends up.</summary>
        internal AIMove Then(string before, string after)
        {
            return new AIMove(HasMove ? Before : before, after);
        }
    }
}
