using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Graph.Nodes
{
    /// <summary>
    /// How an AI Assistant turn changed a node. When one turn changes a node in several ways, the
    /// highest value wins: a node the turn placed stays <see cref="Placed"/> even if the same turn then
    /// sets its value or moves it, and a node whose inputs changed is <see cref="Modified"/> even if it
    /// was also moved.
    /// </summary>
    public enum AIChangeKind
    {
        /// <summary>The node was moved on the canvas; none of its inputs changed.</summary>
        Moved = 0,

        /// <summary>One or more of the node's inputs changed. See <see cref="AINodeChange.InputChanges"/>.</summary>
        Modified = 1,

        /// <summary>The node was added to the graph.</summary>
        Placed = 2,
    }

    /// <summary>
    /// One input of a node that an AI Assistant turn changed, with its value before and after the turn.
    /// An "input" is an input port's wiring or anything else that feeds the node's result, such as a
    /// slider value, Code Block text, a Python script, the Python engine, the node's name or its lacing.
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
        /// Name of the input: a port name for a wiring change, or a fixed name such as "Value", "Code",
        /// "Script", "Engine", "Name" or "Lacing" for the node's own settings.
        /// </summary>
        public string InputName { get; }

        /// <summary>The value before the turn, as display text. Null when there was none.</summary>
        public string Before { get; }

        /// <summary>The value after the turn, as display text. Null when there is none.</summary>
        public string After { get; }
    }

    /// <summary>
    /// What one AI Assistant turn did to one node. A node's current record is
    /// <see cref="NodeModel.AIChange"/>; a turn keeps its own copy of every node it touched, which
    /// <see cref="Dynamo.Graph.Workspaces.AITurnSummary.Nodes"/> returns.
    /// </summary>
    /// <remarks>
    /// The record holds the net effect of the turn: the node before the turn compared with the node
    /// after it. A value that was changed and then changed back, or a node moved back to where it
    /// started, leaves no record. The record is never saved to the .dyn file and never marks the
    /// workspace as having unsaved changes.
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

        /// <summary>Identifier of the turn, as reported by the caller that recorded the change.</summary>
        public string TurnId { get; }

        /// <summary>
        /// Position of the turn among the turns recorded in this workspace, starting at 1. Meant for
        /// display ("Turn 3"); use <see cref="TurnId"/> to identify a turn.
        /// </summary>
        public int TurnNumber { get; }

        /// <summary>
        /// The inputs the turn changed, in the order they were first changed. Empty for a
        /// <see cref="AIChangeKind.Placed"/> or <see cref="AIChangeKind.Moved"/> node.
        /// </summary>
        public IReadOnlyList<AIInputChange> InputChanges { get; }

        /// <summary>Where the turn moved the node from and to; empty when it did not move it.</summary>
        internal AIMove Move { get; }

        /// <summary>
        /// Adds one change to a node's record and returns the resulting record, or null when the turn's
        /// net effect on the node is nothing.
        /// </summary>
        /// <param name="current">The node's record so far in this turn; null when it has none.</param>
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
            // A later turn starts a fresh record: the record describes one turn only.
            if (current != null && !string.Equals(current.TurnId, turnId, StringComparison.Ordinal))
            {
                current = null;
            }

            // Everything about a node the turn placed is new, so there is no before/after to keep.
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
        /// Adds a changed input to <paramref name="inputs"/>, keeping the value the input had before the
        /// turn and following its latest value. An input changed back to its value before the turn is
        /// removed, because it is not a change.
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
    /// One change reported to <see cref="AINodeChange.Apply"/>: what kind it was and, for a modified
    /// input or a move, the value or position before and after it.
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
    /// Where one turn moved a node: the position before its first move and after its last one.
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

        /// <summary>True when the turn moved the node at least once.</summary>
        internal bool HasMove { get; }

        /// <summary>Where the node was before the turn's first move, as reported by the caller.</summary>
        internal string Before { get; }

        /// <summary>Where the turn's last move left the node, as reported by the caller.</summary>
        internal string After { get; }

        /// <summary>
        /// A node moved back to where it started has not moved. Positions the caller did not report
        /// cannot be compared, so such a move always counts.
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
