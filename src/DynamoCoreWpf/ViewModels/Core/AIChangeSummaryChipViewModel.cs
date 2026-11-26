using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.UI.Commands;
using Dynamo.Wpf.Properties;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// Canvas chip summarising the latest Assistant turn, with a stepper through the turn's nodes,
    /// Show all, Undo all and Dismiss. Added and changed count only the nodes that still carry the turn's
    /// marker: a node a person edits or clears with Esc is theirs and drops out. Deleted nodes and removed
    /// connections are the turn's history. The chip can be turned off in Preferences; that does not hide the halo.
    /// </summary>
    internal sealed class AIChangeSummaryChipViewModel : ViewModelBase
    {
        private const string NoStep = "–";

        private readonly WorkspaceViewModel workspaceViewModel;
        private readonly Dispatcher dispatcher;
        private readonly PreferenceSettings preferenceSettings;
        private int refreshPending;
        private bool disposed;

        private bool isVisible;
        private bool canShow;
        private string message = string.Empty;
        private string stepText = string.Empty;
        private bool canUndoAll;
        private bool showUndoAll = true;
        private string undoAllToolTip = string.Empty;
        private string stepTurnId;
        private int stepTurnNumber;
        private Guid? stepNodeId;
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

            preferenceSettings = workspaceViewModel.DynamoViewModel?.Model?.PreferenceSettings;
            if (preferenceSettings != null)
            {
                preferenceSettings.PropertyChanged += OnPreferenceSettingsChanged;
            }

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
        /// True when at least one of the turn's nodes still carries its marker: Show all and the stepper apply.
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
        /// False when the AI client cannot undo a whole turn (an older DynamoMCP): Undo all is hidden.
        /// </summary>
        public bool ShowUndoAll
        {
            get => showUndoAll;
            private set
            {
                if (showUndoAll == value) return;
                showUndoAll = value;
                RaisePropertyChanged(nameof(ShowUndoAll));
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
        /// Show all: zooms to fit the turn's marked nodes and replays their halo.
        /// </summary>
        internal void Show()
        {
            var turn = LatestTurn();
            var nodes = turn == null ? new List<NodeModel>() : MarkedNodes(turn).Select(entry => entry.Node).ToList();
            if (nodes.Count == 0)
            {
                Refresh();
                return;
            }

            foreach (var node in nodes)
            {
                FindNodeViewModel(node)?.ReplayAIHaloGlow();
            }

            ResetStep();
            StepText = FormatStep(-1, nodes.Count);
            workspaceViewModel.FitViewToNodes(nodes);
        }

        /// <summary>
        /// Steps to the next or previous marked node of the turn, left to right, wrapping at either end.
        /// Centres the node (see <see cref="WorkspaceViewModel.StepZoom"/>) and gives its halo one slow breath.
        /// </summary>
        internal void Step(int direction)
        {
            var turn = LatestTurn();
            var nodes = turn == null ? new List<NodeModel>() : StepNodes(MarkedNodes(turn));
            if (nodes.Count == 0)
            {
                Refresh();
                return;
            }

            var current = CurrentStep(nodes);
            int next;
            if (current >= 0)
            {
                next = current + direction;
            }
            else if (stepNodeId != null && stepIndex >= 0)
            {
                // The node the stepper was on dropped out (edited or deleted): carry on from where it was
                next = direction > 0 ? stepIndex : stepIndex - 1;
            }
            else
            {
                next = direction > 0 ? 0 : nodes.Count - 1;
            }

            stepIndex = (next % nodes.Count + nodes.Count) % nodes.Count;
            var node = nodes[stepIndex];
            stepNodeId = node.GUID;
            StepText = FormatStep(stepIndex, nodes.Count);

            FindNodeViewModel(node)?.PulseAIHaloGlow();
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

        private bool IsEnabledInPreferences => preferenceSettings?.ShowAIChangeSummaryChip ?? true;

        private void OnPreferenceSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PreferenceSettings.ShowAIChangeSummaryChip))
            {
                OnAIChangesUpdated();
            }
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
            if (turn == null || turn.IsUndone)
            {
                Hide();
                return;
            }

            if (!string.Equals(stepTurnId, turn.TurnId, StringComparison.Ordinal) || stepTurnNumber != turn.TurnNumber)
            {
                stepTurnId = turn.TurnId;
                stepTurnNumber = turn.TurnNumber;
                ResetStep();
            }

            var marked = MarkedNodes(turn);
            var added = marked.Count(entry => entry.Change.Kind == AIChangeKind.Placed);
            var changed = marked.Count - added;
            var deleted = turn.DeletedNodeCount;
            var removedConnections = turn.RemovedStandaloneConnectorCount;
            var undoAvailability = RefreshUndoAll(turn.TurnId);

            // Close when there is nothing left to describe, or when the person has taken over (or deleted)
            // all of the turn's nodes and the turn can no longer be undone
            var takenOver = turn.Nodes.Count > 0 && marked.Count == 0 && undoAvailability != AIUndoAvailability.Available;
            if (added + changed + deleted + removedConnections == 0 || takenOver)
            {
                Hide();
                return;
            }

            var stepNodes = StepNodes(marked);
            Message = FormatMessage(added, changed, deleted, removedConnections);
            StepText = FormatStep(CurrentStep(stepNodes), stepNodes.Count);
            CanShow = stepNodes.Count > 0;
            IsVisible = IsEnabledInPreferences;
        }

        private void Hide()
        {
            CanShow = false;
            CanUndoAll = false;
            IsVisible = false;
        }

        private void ResetStep()
        {
            stepNodeId = null;
            stepIndex = -1;
        }

        // Where the stepper is in the list, or -1 if it has not stepped yet or its node dropped out
        private int CurrentStep(List<NodeModel> nodes)
        {
            var index = stepNodeId == null ? -1 : nodes.FindIndex(node => node.GUID == stepNodeId.Value);
            if (index >= 0)
            {
                stepIndex = index;
            }

            return index;
        }

        private AIUndoAvailability RefreshUndoAll(string turnId)
        {
            var availability = workspaceViewModel.Model.GetAITurnUndoAvailability(turnId);
            CanUndoAll = availability == AIUndoAvailability.Available;
            ShowUndoAll = availability != AIUndoAvailability.NotSupported;
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
        /// The turn's nodes that are on the canvas and still carry its marker, in the order the Assistant
        /// touched them, each with what the turn did to it.
        /// </summary>
        private List<(NodeModel Node, AINodeChange Change)> MarkedNodes(AITurnSummary turn)
        {
            var nodesById = workspaceViewModel.Model.Nodes
                .GroupBy(n => n.GUID)
                .ToDictionary(g => g.Key, g => g.First());

            return turn.NodeIds
                .Zip(turn.Nodes, (id, change) => (Node: nodesById.TryGetValue(id, out var node) ? node : null, Change: change))
                .Where(entry => entry.Node != null && IsMarkedBy(entry.Node, turn))
                .ToList();
        }

        // The stepper's order: left to right, then top to bottom
        private static List<NodeModel> StepNodes(List<(NodeModel Node, AINodeChange Change)> marked)
        {
            return marked.Select(entry => entry.Node).OrderBy(node => node.X).ThenBy(node => node.Y).ToList();
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

            if (preferenceSettings != null)
            {
                preferenceSettings.PropertyChanged -= OnPreferenceSettingsChanged;
            }

            base.Dispose();
        }
    }
}
