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
    /// What the most recent AI Assistant turn that touched a node did to it. Read it from
    /// <see cref="NodeModel.AIChange"/>.
    /// </summary>
    /// <remarks>
    /// The record holds the net effect of the turn: the node before the turn compared with the node
    /// after it. A value that was changed and then changed back, or a node moved back to where it
    /// started, leaves no record. The record is never saved to the .dyn file and never marks the
    /// workspace as having unsaved changes.
    /// </remarks>
    public sealed class AINodeChange
    {
        /// <summary>Input name used when a caller reports a modification without naming the input.</summary>
        internal const string UnnamedInput = "(node)";

        private static readonly IReadOnlyList<AIInputChange> NoInputs = Array.Empty<AIInputChange>();

        private AINodeChange(
            AIChangeKind kind,
            string turnId,
            int turnNumber,
            IReadOnlyList<AIInputChange> inputChanges,
            bool hasMove,
            string positionBefore,
            string positionAfter)
        {
            Kind = kind;
            TurnId = turnId;
            TurnNumber = turnNumber;
            InputChanges = inputChanges;
            HasMove = hasMove;
            PositionBefore = positionBefore;
            PositionAfter = positionAfter;
        }

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

        /// <summary>True when the turn moved the node at least once.</summary>
        internal bool HasMove { get; }

        /// <summary>Where the node was before the turn's first move, as reported by the caller.</summary>
        internal string PositionBefore { get; }

        /// <summary>Where the turn's last move left the node, as reported by the caller.</summary>
        internal string PositionAfter { get; }

        /// <summary>
        /// Adds one change to a node's record and returns the resulting record, or null when the turn's
        /// net effect on the node is nothing.
        /// </summary>
        /// <param name="current">The node's record so far; null when it has none.</param>
        /// <param name="kind">What the change was.</param>
        /// <param name="turnId">Turn that made the change.</param>
        /// <param name="turnNumber">Display number of that turn.</param>
        /// <param name="inputName">For <see cref="AIChangeKind.Modified"/>: the input that changed.</param>
        /// <param name="before">The input's value (or for a move, the position) before the change.</param>
        /// <param name="after">The input's value (or for a move, the position) after the change.</param>
        internal static AINodeChange Apply(
            AINodeChange current,
            AIChangeKind kind,
            string turnId,
            int turnNumber,
            string inputName,
            string before,
            string after)
        {
            // A later turn starts a fresh record: the record describes the most recent turn only.
            if (current != null && !string.Equals(current.TurnId, turnId, StringComparison.Ordinal))
            {
                current = null;
            }

            // Everything about a node the turn placed is new, so there is no before/after to keep.
            if (kind == AIChangeKind.Placed || current?.Kind == AIChangeKind.Placed)
            {
                return new AINodeChange(AIChangeKind.Placed, turnId, turnNumber, NoInputs, false, null, null);
            }

            var inputs = current?.InputChanges.ToList() ?? new List<AIInputChange>();
            var hasMove = current?.HasMove ?? false;
            var positionBefore = current?.PositionBefore;
            var positionAfter = current?.PositionAfter;

            if (kind == AIChangeKind.Moved)
            {
                // Keep where the node started the turn; follow where it ends up.
                if (!hasMove)
                {
                    positionBefore = before;
                }

                positionAfter = after;
                hasMove = true;
            }
            else
            {
                var unnamed = string.IsNullOrEmpty(inputName);
                var name = unnamed ? UnnamedInput : inputName;
                var index = inputs.FindIndex(i => string.Equals(i.InputName, name, StringComparison.Ordinal));

                // Keep the value the input had before the turn; follow its latest value.
                var turnBefore = index >= 0 ? inputs[index].Before : before;
                if (index >= 0)
                {
                    inputs.RemoveAt(index);
                }

                // An input changed and then changed back is not a change. A modification reported
                // without an input name cannot be compared, so it is always kept.
                if (unnamed || !string.Equals(turnBefore, after, StringComparison.Ordinal))
                {
                    inputs.Insert(index >= 0 ? index : inputs.Count, new AIInputChange(name, turnBefore, after));
                }
            }

            if (inputs.Count > 0)
            {
                return new AINodeChange(AIChangeKind.Modified, turnId, turnNumber, inputs.AsReadOnly(), hasMove, positionBefore, positionAfter);
            }

            // A node moved back to where it started has not moved. Positions the caller did not
            // report cannot be compared, so such a move is always kept.
            var moved = hasMove
                && !(positionBefore != null && string.Equals(positionBefore, positionAfter, StringComparison.Ordinal));
            if (moved)
            {
                return new AINodeChange(AIChangeKind.Moved, turnId, turnNumber, NoInputs, hasMove, positionBefore, positionAfter);
            }

            return null;
        }
    }
}
