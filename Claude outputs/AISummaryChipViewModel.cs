using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.UI.Commands;
using Dynamo.Wpf.Properties;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// The AI summary chip on the canvas (DYN-11011, Figma node 7:1137): "Autodesk Assistant changed
    /// 12 nodes · 2 deleted", with Show and Dismiss. The halo on each node fades after a few seconds,
    /// so the chip is how a person who was looking elsewhere still finds out what the Assistant did.
    /// It is also the only place deleted nodes are reported, since they cannot be highlighted.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Describes the workspace's latest Assistant turn (<see cref="WorkspaceModel.GetAITurnSummaries"/>).
    /// It appears when that turn placed, modified, moved or deleted at least one node, and updates as
    /// the turn goes on. A turn that only changed wires, groups or notes shows no chip: a wire change
    /// already marks the node whose input it feeds as modified (D9).
    /// </para>
    /// <para>
    /// The counts are the turn's history: a person editing one of the nodes afterwards does not change
    /// them. The chip stays until Dismiss or the next Assistant turn. Show only re-lights nodes that
    /// still carry the turn's marker, so nodes a person has edited since (and now owns) stay dark.
    /// </para>
    /// <para>
    /// Never changes the graph: no undo step, never marks the workspace unsaved, never saved.
    /// </para>
    /// </remarks>
    internal sealed class AISummaryChipViewModel : ViewModelBase
    {
        private readonly WorkspaceViewModel workspaceViewModel;

        private bool isVisible;
        private string text = string.Empty;
        private int changedNodeCount;
        private int deletedNodeCount;
        private bool canShow;
        private string turnId;

        internal AISummaryChipViewModel(WorkspaceViewModel workspaceViewModel)
        {
            this.workspaceViewModel = workspaceViewModel ?? throw new ArgumentNullException(nameof(workspaceViewModel));

            ShowCommand = new DelegateCommand(_ => Show(), _ => CanShow);
            DismissCommand = new DelegateCommand(_ => Dismiss(), _ => IsVisible);

            Workspace.AIChangesUpdated += Refresh;
            Workspace.NodeRemoved += OnNodeRemoved;
            Refresh();
        }

        private WorkspaceModel Workspace => workspaceViewModel.Model;

        #region What the chip shows

        /// <summary>
        /// True while the latest Assistant turn has placed, modified, moved or deleted a node.
        /// </summary>
        public bool IsVisible
        {
            get => isVisible;
            private set
            {
                if (isVisible == value) return;
                isVisible = value;
                RaisePropertyChanged(nameof(IsVisible));
                DismissCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// The sentence on the chip, for example "Autodesk Assistant changed 12 nodes · 2 deleted".
        /// Empty while the chip is hidden.
        /// </summary>
        public string Text
        {
            get => text;
            private set
            {
                if (text == value) return;
                text = value;
                RaisePropertyChanged(nameof(Text));
            }
        }

        /// <summary>
        /// How many nodes the latest turn placed, modified or moved.
        /// </summary>
        public int ChangedNodeCount
        {
            get => changedNodeCount;
            private set
            {
                if (changedNodeCount == value) return;
                changedNodeCount = value;
                RaisePropertyChanged(nameof(ChangedNodeCount));
            }
        }

        /// <summary>
        /// How many nodes that existed before the latest turn it deleted.
        /// </summary>
        public int DeletedNodeCount
        {
            get => deletedNodeCount;
            private set
            {
                if (deletedNodeCount == value) return;
                deletedNodeCount = value;
                RaisePropertyChanged(nameof(DeletedNodeCount));
            }
        }

        /// <summary>
        /// True when Show has something to show: at least one node still carries the latest turn's
        /// marker. False when the turn only deleted nodes, or a person has since edited every node it
        /// changed. The Show button is hidden while this is false.
        /// </summary>
        public bool CanShow
        {
            get => canShow;
            private set
            {
                if (canShow == value) return;
                canShow = value;
                RaisePropertyChanged(nameof(CanShow));
                ShowCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Re-lights the halo on the latest turn's nodes and zooms the canvas to fit them.
        /// </summary>
        public DelegateCommand ShowCommand { get; }

        /// <summary>
        /// Clears every AI highlight in the workspace and hides the chip. Does not change the graph.
        /// </summary>
        public DelegateCommand DismissCommand { get; }

        /// <summary>
        /// The turn the chip describes, or null while hidden. For tests.
        /// </summary>
        internal string TurnId => turnId;

        /// <summary>
        /// Builds the chip's sentence from the two counts. Returns an empty string when both are zero.
        /// </summary>
        /// <param name="changed">Nodes the turn placed, modified or moved.</param>
        /// <param name="deleted">Nodes the turn deleted.</param>
        internal static string BuildText(int changed, int deleted)
        {
            var culture = CultureInfo.CurrentCulture;

            if (changed <= 0)
            {
                if (deleted <= 0) return string.Empty;

                return deleted == 1
                    ? Resources.AISummaryChipDeletedOneNode
                    : string.Format(culture, Resources.AISummaryChipDeletedNodes, deleted);
            }

            var changedText = changed == 1
                ? Resources.AISummaryChipChangedOneNode
                : string.Format(culture, Resources.AISummaryChipChangedNodes, changed);

            return deleted <= 0
                ? changedText
                : string.Format(culture, Resources.AISummaryChipChangedAndDeleted, changedText, deleted);
        }

        #endregion

        #region Keeping up with the record

        /// <summary>
        /// Reads the latest turn again and updates what the chip shows.
        /// </summary>
        internal void Refresh()
        {
            var turn = Workspace.GetAITurnSummaries().LastOrDefault();
            var changed = turn?.Nodes.Count ?? 0;
            var deleted = turn?.DeletedNodeCount ?? 0;

            turnId = changed > 0 || deleted > 0 ? turn.TurnId : null;
            ChangedNodeCount = turnId == null ? 0 : changed;
            DeletedNodeCount = turnId == null ? 0 : deleted;
            Text = BuildText(ChangedNodeCount, DeletedNodeCount);
            CanShow = turnId != null && NodesToShow().Any();
            IsVisible = turnId != null;
        }

        private void OnNodeRemoved(NodeModel node)
        {
            // A person deleting a marked node can leave nothing to show
            if (turnId != null)
            {
                CanShow = NodesToShow().Any();
            }
        }

        /// <summary>
        /// The nodes Show lights up: those still in the workspace that carry the latest turn's marker,
        /// in the order the turn first touched them.
        /// </summary>
        internal IReadOnlyList<NodeModel> NodesToShow()
        {
            if (turnId == null) return Array.Empty<NodeModel>();

            var turn = Workspace.GetAITurnSummary(turnId);
            if (turn == null) return Array.Empty<NodeModel>();

            var marked = Workspace.Nodes
                .Where(n => n.AIChange != null && string.Equals(n.AIChange.TurnId, turnId, StringComparison.Ordinal))
                .ToDictionary(n => n.GUID);

            return turn.NodeIds
                .Where(marked.ContainsKey)
                .Select(id => marked[id])
                .ToList()
                .AsReadOnly();
        }

        #endregion

        #region Show and Dismiss

        /// <summary>
        /// Re-lights the halo on the latest turn's nodes and zooms to fit them. The person's selection
        /// is left as it is.
        /// </summary>
        internal void Show()
        {
            var nodes = NodesToShow();
            if (nodes.Count == 0) return;

            var ids = new HashSet<Guid>(nodes.Select(n => n.GUID));
            foreach (var nodeViewModel in workspaceViewModel.Nodes.Where(vm => ids.Contains(vm.NodeModel.GUID)))
            {
                nodeViewModel.ReplayAIHaloGlow();
            }

            workspaceViewModel.FitViewToNodes(nodes);
        }

        /// <summary>
        /// Clears every AI highlight in the workspace. The record's change notice then hides the chip.
        /// </summary>
        internal void Dismiss()
        {
            Workspace.ClearAIChanges();
        }

        #endregion

        public override void Dispose()
        {
            Workspace.AIChangesUpdated -= Refresh;
            Workspace.NodeRemoved -= OnNodeRemoved;
            base.Dispose();
        }
    }
}
