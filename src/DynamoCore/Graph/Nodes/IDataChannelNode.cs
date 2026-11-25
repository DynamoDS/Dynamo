using System;
using System.Collections.Generic;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Graph.Nodes
{
    /// <summary>
    /// Describes the current resolution state of a data channel node within its workspace.
    /// </summary>
    public enum DataChannelBindingStatus
    {
        /// <summary>
        /// The node has no channel key or pattern configured.
        /// </summary>
        Unassigned,

        /// <summary>
        /// The channel key or pattern is active and matched.
        /// </summary>
        Bound,

        /// <summary>
        /// No sender in the workspace matches the requested channel pattern.
        /// </summary>
        MissingSender,

        /// <summary>
        /// Binding to a matching sender would create a cyclic dependency in the graph.
        /// </summary>
        CyclicDependency
    }

    /// <summary>
    /// Common contract for nodes that participate in named, workspace-scoped logical data channels.
    /// </summary>
    public interface IDataChannelNode
    {
        /// <summary>
        /// Gets the channel key (on a sender) or wildcard pattern (on a receiver) used for matching in the same workspace.
        /// </summary>
        string ChannelName { get; }

        /// <summary>
        /// Raised when <see cref="ChannelName"/> changes so the owning workspace can reconcile bindings.
        /// </summary>
        event Action<NodeModel> ChannelNameChanged;
    }

    /// <summary>
    /// Represents a node that publishes its evaluated output onto a named logical channel within its workspace.
    /// </summary>
    public interface IDataChannelSender : IDataChannelNode
    {
        /// <summary>
        /// Gets the DesignScript AST identifier holding the value published by this sender.
        /// </summary>
        /// <returns>The AST identifier referenced by bound receivers.</returns>
        IdentifierNode GetChannelOutputAstIdentifier();

        /// <summary>
        /// Updates the sender's binding status as determined by workspace channel reconciliation.
        /// </summary>
        /// <param name="status">The resolved channel status for this sender.</param>
        void UpdateChannelBinding(DataChannelBindingStatus status);
    }

    /// <summary>
    /// Represents a node that receives data from one or more matching <see cref="IDataChannelSender"/> nodes
    /// in the same workspace.
    /// </summary>
    public interface IDataChannelReceiver : IDataChannelNode
    {
        /// <summary>
        /// Gets the deterministically ordered list of currently bound sender nodes.
        /// </summary>
        IReadOnlyList<NodeModel> BoundSenders { get; }

        /// <summary>
        /// Updates the receiver's bound senders and status as determined by workspace channel reconciliation.
        /// </summary>
        /// <param name="senders">The matched sender nodes in deterministic order (empty when unbound).</param>
        /// <param name="status">The resolved channel status for this receiver.</param>
        /// <returns><see langword="true"/> if the bound sender sequence changed and AST recompilation is required.</returns>
        bool UpdateChannelBinding(IReadOnlyList<NodeModel> senders, DataChannelBindingStatus status);

        /// <summary>
        /// Updates the list of available channel names currently defined by senders in the same workspace.
        /// </summary>
        /// <param name="availableChannels">Sorted distinct non-empty sender channel names.</param>
        void UpdateAvailableChannels(IReadOnlyList<string> availableChannels);
    }
}
