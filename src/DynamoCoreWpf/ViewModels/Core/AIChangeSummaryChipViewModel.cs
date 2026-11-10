using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.UI.Commands;
using Dynamo.Wpf.Properties;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// Canvas chip summarising the latest Assistant turn, e.g. "Autodesk Assistant added 3 nodes · changed 9 ·
    /// deleted 2" (DYN-11011). Hidden when the turn changed nothing, or when no node still carries the turn's marker
    /// and nothing was deleted. The text does not change when a person edits a node afterwards.
    /// </summary>
    internal sealed class AIChangeSummaryChipViewModel : ViewModelBase
    {
        private readonly WorkspaceViewModel workspaceViewModel;
        private readonly Dispatcher dispatcher;
        private int refreshPending;
        private bool disposed;

        private bool isVisible;
        private bool canShow;
        private string message = string.Empty;

        internal AIChangeSummaryChipViewModel(WorkspaceViewModel workspaceViewModel)
        {
            this.workspaceViewModel = workspaceViewModel ?? throw new ArgumentNullException(nameof(workspaceViewModel));
            dispatcher = Dispatcher.CurrentDispatcher;

            ShowCommand = new DelegateCommand(_ => Show(), _ => CanShow);
            DismissCommand = new DelegateCommand(_ => Dismiss(), _ => IsVisible);

            var model = workspaceViewModel.Model;
            model.AIChangesUpdated += OnAIChangesUpdated;
            model.NodeRemoved += OnNodeRemoved;
            model.NodesCleared += OnAIChangesUpdated;

            Refresh();
        }

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
        /// True when at least one node still carries the turn's marker.
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

        public string Message
        {
            get => message;
            private set
            {
                if (message == value) return;
                message = value;
                RaisePropertyChanged(nameof(Message));
            }
        }

        public DelegateCommand ShowCommand { get; }

        public DelegateCommand DismissCommand { get; }

        /// <summary>
        /// Zooms to fit the turn's nodes still on the canvas, and replays the halo on those that still carry
        /// the turn's marker. Nodes a person has edited since are framed but do not glow.
        /// </summary>
        internal void Show()
        {
            var turn = LatestTurn();
            var nodes = turn == null ? new List<NodeModel>() : TurnNodes(turn);
            var marked = nodes.Where(n => IsMarkedBy(n, turn)).ToList();
            if (marked.Count == 0)
            {
                Refresh();
                return;
            }

            var nodeViewModels = workspaceViewModel.Nodes
                .Where(n => n?.NodeModel != null)
                .GroupBy(n => n.NodeModel.GUID)
                .ToDictionary(g => g.Key, g => g.First());

            var replayGroup = AIHaloGlow.NewReplayGroup();
            foreach (var node in marked)
            {
                if (nodeViewModels.TryGetValue(node.GUID, out var nodeViewModel))
                {
                    nodeViewModel.ReplayAIHaloGlow(replayGroup);
                }
            }

            workspaceViewModel.FitViewToNodes(nodes);
        }

        /// <summary>
        /// Clears all AI highlights. Does not touch the graph, undo history or saved state.
        /// </summary>
        internal void Dismiss()
        {
            IsVisible = false;
            workspaceViewModel.Model.ClearAIChanges();
        }

        private void OnNodeRemoved(NodeModel node)
        {
            OnAIChangesUpdated();
        }

        private void OnAIChangesUpdated()
        {
            if (disposed || Interlocked.Exchange(ref refreshPending, 1) == 1) return;

            dispatcher.BeginInvoke(new Action(() =>
            {
                Interlocked.Exchange(ref refreshPending, 0);
                if (!disposed)
                {
                    Refresh();
                }
            }), DispatcherPriority.Background);
        }

        internal void Refresh()
        {
            var turn = LatestTurn();
            var added = turn?.Nodes.Count(n => n.Kind == AIChangeKind.Placed) ?? 0;
            var changed = (turn?.Nodes.Count ?? 0) - added;
            var deleted = turn?.DeletedNodeCount ?? 0;
            var stillMarked = turn == null ? 0 : TurnNodes(turn).Count(n => IsMarkedBy(n, turn));

            if (stillMarked == 0 && deleted == 0)
            {
                CanShow = false;
                IsVisible = false;
                return;
            }

            Message = FormatMessage(added, changed, deleted);
            CanShow = stillMarked > 0;
            IsVisible = true;
        }

        private const string MessageSeparator = " · ";

        /// <summary>
        /// The chip's sentence. Groups with no nodes are left out. The first group names the nodes
        /// ("added 3 nodes"); the rest follow as short parts ("changed 9"). Moved nodes count as changed.
        /// </summary>
        internal static string FormatMessage(int added, int changed, int deleted)
        {
            var culture = CultureInfo.CurrentCulture;
            var parts = new List<string>();

            void Add(int count, string one, string many, string followUp)
            {
                if (count <= 0) return;
                parts.Add(parts.Count > 0
                    ? string.Format(culture, followUp, count)
                    : count == 1 ? one : string.Format(culture, many, count));
            }

            Add(added, Resources.AIChangeSummaryChipAddedOneNode, Resources.AIChangeSummaryChipAddedNodes, null);
            Add(changed, Resources.AIChangeSummaryChipChangedOneNode, Resources.AIChangeSummaryChipChangedNodes, Resources.AIChangeSummaryChipChangedCount);
            Add(deleted, Resources.AIChangeSummaryChipDeletedOneNode, Resources.AIChangeSummaryChipDeletedNodes, Resources.AIChangeSummaryChipDeletedCount);

            return string.Join(MessageSeparator, parts);
        }

        private AITurnSummary LatestTurn()
        {
            return workspaceViewModel.Model.GetAITurnSummaries().LastOrDefault();
        }

        /// <summary>
        /// The turn's nodes that are still on the canvas, in turn order.
        /// </summary>
        private List<NodeModel> TurnNodes(AITurnSummary turn)
        {
            var nodesById = workspaceViewModel.Model.Nodes
                .GroupBy(n => n.GUID)
                .ToDictionary(g => g.Key, g => g.First());

            return turn.NodeIds
                .Select(id => nodesById.TryGetValue(id, out var node) ? node : null)
                .Where(node => node != null)
                .ToList();
        }

        private static bool IsMarkedBy(NodeModel node, AITurnSummary turn)
        {
            return node.AIChange != null
                && string.Equals(node.AIChange.TurnId, turn.TurnId, StringComparison.Ordinal);
        }

        public override void Dispose()
        {
            if (disposed) return;
            disposed = true;

            var model = workspaceViewModel.Model;
            model.AIChangesUpdated -= OnAIChangesUpdated;
            model.NodeRemoved -= OnNodeRemoved;
            model.NodesCleared -= OnAIChangesUpdated;

            base.Dispose();
        }
    }
}
