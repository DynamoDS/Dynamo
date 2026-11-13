using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Canvas chip summarising the latest Assistant turn (DYN-11011), with a stepper through the turn's
    /// nodes, Show all, Undo all and Dismiss (DYN-11012). The counts are the turn's history: later edits
    /// do not change them. The chip stays until Dismiss, Undo all or the next Assistant turn.
    /// </summary>
    internal sealed class AIChangeSummaryChipViewModel : ViewModelBase
    {
        private const string NoStep = "–";

        private readonly WorkspaceViewModel workspaceViewModel;
        private readonly Dispatcher dispatcher;
        private int refreshPending;
        private bool disposed;

        private bool isVisible;
        private bool canShow;
        private string message = string.Empty;
        private string stepText = string.Empty;
        private bool canUndoAll;
        private string undoAllToolTip = string.Empty;
        private string stepTurnId;
        private int stepIndex = -1;

        internal AIChangeSummaryChipViewModel(WorkspaceViewModel workspaceViewModel)
        {
            this.workspaceViewModel = workspaceViewModel ?? throw new ArgumentNullException(nameof(workspaceViewModel));
            dispatcher = Dispatcher.CurrentDispatcher;

            ShowCommand = new DelegateCommand(_ => Show(), _ => CanShow);
            PreviousCommand = new DelegateCommand(_ => Step(-1), _ => CanShow);
            NextCommand = new DelegateCommand(_ => Step(1), _ => CanShow);
            UndoAllCommand = new DelegateCommand(_ => UndoAll(), _ => CanUndoAll);
            DismissCommand = new DelegateCommand(_ => Dismiss(), _ => IsVisible);

            var model = workspaceViewModel.Model;
            model.AIChangesUpdated += OnAIChangesUpdated;
            model.NodeAdded += OnNodeAddedOrRemoved;
            model.NodeRemoved += OnNodeAddedOrRemoved;
            model.NodesCleared += OnAIChangesUpdated;
            model.PropertyChanged += OnWorkspacePropertyChanged;

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
        /// True when at least one of the turn's nodes is still on the canvas: Show all and the stepper apply.
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
                PreviousCommand.RaiseCanExecuteChanged();
                NextCommand.RaiseCanExecuteChanged();
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

        /// <summary>
        /// The stepper's position, e.g. "3 of 12".
        /// </summary>
        public string StepText
        {
            get => stepText;
            private set
            {
                if (stepText == value) return;
                stepText = value;
                RaisePropertyChanged(nameof(StepText));
            }
        }

        public bool CanUndoAll
        {
            get => canUndoAll;
            private set
            {
                if (canUndoAll == value) return;
                canUndoAll = value;
                RaisePropertyChanged(nameof(CanUndoAll));
                UndoAllCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// What Undo all does, or why it is unavailable.
        /// </summary>
        public string UndoAllToolTip
        {
            get => undoAllToolTip;
            private set
            {
                if (undoAllToolTip == value) return;
                undoAllToolTip = value;
                RaisePropertyChanged(nameof(UndoAllToolTip));
            }
        }

        public DelegateCommand ShowCommand { get; }

        public DelegateCommand PreviousCommand { get; }

        public DelegateCommand NextCommand { get; }

        public DelegateCommand UndoAllCommand { get; }

        public DelegateCommand DismissCommand { get; }

        /// <summary>
        /// Show all: zooms to fit the turn's nodes still on the canvas, and replays the halo on those that
        /// still carry the turn's marker. Nodes a person has edited since are framed but do not glow.
        /// </summary>
        internal void Show()
        {
            var turn = LatestTurn();
            var nodes = turn == null ? new List<NodeModel>() : TurnNodes(turn);
            if (nodes.Count == 0)
            {
                Refresh();
                return;
            }

            foreach (var node in nodes.Where(n => IsMarkedBy(n, turn)))
            {
                FindNodeViewModel(node)?.ReplayAIHaloGlow();
            }

            stepIndex = -1;
            StepText = FormatStep(stepIndex, nodes.Count);
            workspaceViewModel.FitViewToNodes(nodes);
        }

        /// <summary>
        /// Steps to the next (1) or previous (-1) node of the turn, in the order the Assistant touched them,
        /// wrapping at either end. Centres the node at 100% zoom and gives its halo one slow breath.
        /// </summary>
        internal void Step(int direction)
        {
            var turn = LatestTurn();
            var nodes = turn == null ? new List<NodeModel>() : TurnNodes(turn);
            if (nodes.Count == 0)
            {
                Refresh();
                return;
            }

            stepIndex = stepIndex < 0
                ? (direction > 0 ? 0 : nodes.Count - 1)
                : ((stepIndex + direction) % nodes.Count + nodes.Count) % nodes.Count;
            StepText = FormatStep(stepIndex, nodes.Count);

            var node = nodes[stepIndex];
            if (IsMarkedBy(node, turn))
            {
                FindNodeViewModel(node)?.PulseAIHaloGlow();
            }

            workspaceViewModel.CenterViewOnNode(node);
        }

        /// <summary>
        /// Undo all: reverts the whole turn as one undo step (DYN-11008), which closes the chip.
        /// </summary>
        internal void UndoAll()
        {
            var turn = LatestTurn();
            if (turn != null)
            {
                workspaceViewModel.Model.UndoAITurn(turn.TurnId);
            }

            Refresh();
        }

        /// <summary>
        /// Clears all AI highlights. Does not touch the graph, undo history or saved state.
        /// </summary>
        internal void Dismiss()
        {
            IsVisible = false;
            workspaceViewModel.Model.ClearAIChanges();
        }

        private void OnNodeAddedOrRemoved(NodeModel node)
        {
            OnAIChangesUpdated();
        }

        private void OnWorkspacePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CanUndoRedoCommand")
            {
                OnAIChangesUpdated();
            }
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
            var removedConnections = turn?.RemovedStandaloneConnectorCount ?? 0;

            if (added + changed + deleted + removedConnections == 0 || turn.IsUndone)
            {
                CanShow = false;
                CanUndoAll = false;
                IsVisible = false;
                return;
            }

            if (!string.Equals(stepTurnId, turn.TurnId, StringComparison.Ordinal))
            {
                stepTurnId = turn.TurnId;
                stepIndex = -1;
            }

            var stepCount = TurnNodes(turn).Count;
            stepIndex = Math.Min(stepIndex, stepCount - 1);

            Message = FormatMessage(added, changed, deleted, removedConnections);
            StepText = FormatStep(stepIndex, stepCount);
            CanShow = stepCount > 0;
            var undoAvailability = RefreshUndoAll(turn.TurnId);

            // Nothing left to act on: the turn's nodes are all gone and it cannot be undone. Worked out on
            // every refresh, so the chip comes back if the nodes do (Ctrl+Z).
            IsVisible = !(turn.Nodes.Count > 0 && stepCount == 0 && undoAvailability != AIUndoAvailability.Available);
        }

        private AIUndoAvailability RefreshUndoAll(string turnId)
        {
            var availability = workspaceViewModel.Model.GetAITurnUndoAvailability(turnId);
            CanUndoAll = availability == AIUndoAvailability.Available;
            UndoAllToolTip = availability switch
            {
                AIUndoAvailability.Available => Resources.AIChangeSummaryChipUndoAllToolTip,
                AIUndoAvailability.AssistantWorking => Resources.AIChangeSummaryChipUndoAllWorking,
                AIUndoAvailability.EditedSince => Resources.AIChangeSummaryChipUndoAllEditedSince,
                AIUndoAvailability.EditedDuringTurn => Resources.AIChangeSummaryChipUndoAllEditedDuringTurn,
                _ => Resources.AIChangeSummaryChipUndoAllUnavailable,
            };
            return availability;
        }

        private const string MessageSeparator = " · ";

        /// <summary>
        /// The chip's sentence. Empty groups are left out. The first group is a full phrase ("added 3 nodes");
        /// the rest are short ("changed 9"). Moved nodes count as changed.
        /// </summary>
        internal static string FormatMessage(int added, int changed, int deleted, int removedConnections = 0)
        {
            var culture = CultureInfo.CurrentCulture;
            var parts = new List<string>();

            void Add(int count, string one, string many, string followUp, string followUpOne = null)
            {
                if (count <= 0) return;

                string part;
                if (parts.Count > 0)
                {
                    part = count == 1 && followUpOne != null
                        ? followUpOne
                        : string.Format(culture, followUp, count);
                }
                else if (count == 1)
                {
                    part = one;
                }
                else
                {
                    part = string.Format(culture, many, count);
                }

                parts.Add(part);
            }

            Add(added, Resources.AIChangeSummaryChipAddedOneNode, Resources.AIChangeSummaryChipAddedNodes, null);
            Add(changed, Resources.AIChangeSummaryChipChangedOneNode, Resources.AIChangeSummaryChipChangedNodes, Resources.AIChangeSummaryChipChangedCount);
            Add(deleted, Resources.AIChangeSummaryChipDeletedOneNode, Resources.AIChangeSummaryChipDeletedNodes, Resources.AIChangeSummaryChipDeletedCount);
            Add(removedConnections, Resources.AIChangeSummaryChipRemovedOneConnection, Resources.AIChangeSummaryChipRemovedConnections,
                Resources.AIChangeSummaryChipRemovedConnectionsCount, Resources.AIChangeSummaryChipRemovedOneConnectionCount);

            return string.Join(MessageSeparator, parts);
        }

        /// <summary>
        /// "3 of 12", or "– of 12" before the first step.
        /// </summary>
        internal static string FormatStep(int stepIndex, int stepCount)
        {
            var culture = CultureInfo.CurrentCulture;
            var position = stepIndex < 0 ? NoStep : (stepIndex + 1).ToString(culture);
            return string.Format(culture, Resources.AIChangeSummaryChipStepPosition, position, stepCount);
        }

        private AITurnSummary LatestTurn()
        {
            return workspaceViewModel.Model.GetAITurnSummaries().LastOrDefault();
        }

        /// <summary>
        /// The turn's nodes that are still on the canvas, in the order the Assistant touched them.
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

        private NodeViewModel FindNodeViewModel(NodeModel node)
        {
            return workspaceViewModel.Nodes.FirstOrDefault(n => n?.NodeModel?.GUID == node.GUID);
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
            model.NodeAdded -= OnNodeAddedOrRemoved;
            model.NodeRemoved -= OnNodeAddedOrRemoved;
            model.NodesCleared -= OnAIChangesUpdated;
            model.PropertyChanged -= OnWorkspacePropertyChanged;

            base.Dispose();
        }
    }
}
