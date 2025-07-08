using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.NodeAutoComplete.ViewModels;

namespace Dynamo.NodeAutoComplete.Views
{
    /// <summary>
    /// Interaction logic for NodeAutoCompleteBarView.xaml
    /// </summary>
    public partial class NodeAutoCompleteBarView
    {
        private NodeAutoCompleteBarViewModel ViewModel => DataContext as NodeAutoCompleteBarViewModel;

        private static NodeAutoCompleteBarView _controlInstance;

        // Prepare the autocomplete bar window and reuse it whenever possible.
        // Only a single instance of the window will be allowed at any given time.
        static internal void PrepareAndShowNodeAutoCompleteBar(Window window, NodeAutoCompleteBarViewModel viewModel)
        {
            // A new window will be created (replacing any existing one) whenever a new viewModel is provided.
            // Each workspace gets its own viewModel. For example, when opening a custom node alongside the current workspace.
            if (_controlInstance is null || !ReferenceEquals(_controlInstance.ViewModel, viewModel))
            {
                _controlInstance?.ResetNodeAutoCompleteBar();
                _controlInstance = new NodeAutoCompleteBarView(window, viewModel);
            }

            // When a window is already open, adjust its position to the target port without repeating the full event subscription setup.
            if (_controlInstance?.IsVisible is true)
            {
                //Analytics.TrackEvent(Actions.Open, Categories.NodeAutoCompleteOperations);
                if (_controlInstance?.ViewModel?.PortViewModel != null)
                {
                    _controlInstance.ViewModel.PortViewModel.Highlight = Visibility.Visible;
                    _controlInstance.ViewModel.PortViewModel?.SetupNodeAutoCompleteClusterWindowPlacement(_controlInstance);
                }

                _controlInstance?.ViewModel?.PopulateAutoComplete();
            }
            else
            {
                _controlInstance?.OnShowNodeAutoCompleteBar();
            }
        }

        public NodeAutoCompleteBarView(Window window, NodeAutoCompleteBarViewModel viewModel)
        {
            Owner = window;
            DataContext = viewModel;
            InitializeComponent();
            SubscribeToAppEvents();
        }

        private void OnRefocusSearchbox()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                AutoCompleteSearchBox.Focus();
            }), DispatcherPriority.ApplicationIdle);
        }

        private void OwnerMoved(object sender, EventArgs e) => UpdatePosition();

        private void UpdatePosition()
        {
            ViewModel.PortViewModel.SetupNodeAutoCompleteClusterWindowPlacement(this);
        }

        private void Owner_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NodeModel.Position))
            {
                UpdatePosition();
            }
        }

        // When triggered, they will result in the autocomplete bar window being destroyed.
        private void SubscribeToAppEvents()
        {
            if (string.IsNullOrEmpty(DynamoModel.HostAnalyticsInfo.HostName) && Application.Current != null)
            {
                if (Application.Current?.MainWindow != null)
                {
                    Application.Current.MainWindow.Closing += OnMainAppClosing;
                }
            }

            HomeWorkspaceModel.WorkspaceClosed += OnWorkspaceClosed; //Only hits for main workspace (does not hit for custom nodes workspace).
            ViewModel.dynamoViewModel.Model.WorkspaceHidden += OnWorkspaceRemoved; //De-activating current workspace (including custom node workspace).
            ViewModel.dynamoViewModel.Model.WorkspaceRemoveStarted += OnWorkspaceRemoved; //Closing custom node workspace.
        }

        private void UnsubscribeFromAppEvents()
        {
            if (string.IsNullOrEmpty(DynamoModel.HostAnalyticsInfo.HostName) && Application.Current != null)
            {
                if (Application.Current?.MainWindow != null)
                {
                    Application.Current.MainWindow.Closing -= OnMainAppClosing;
                }
            }

            HomeWorkspaceModel.WorkspaceClosed -= OnWorkspaceClosed;
            ViewModel.dynamoViewModel.Model.WorkspaceHidden -= OnWorkspaceRemoved;
            ViewModel.dynamoViewModel.Model.WorkspaceRemoveStarted -= OnWorkspaceRemoved;
        }

        void WorkspaceModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "X":
                case "Y":
                case "Zoom":
                    Dispatcher.BeginInvoke(UpdatePosition, DispatcherPriority.Loaded);
                    break;
            }
        }

        //Hide the window and unsubscribe from model events.
        //Note that the window is not destroyed, it is just hidden so that it can be reused.
        internal void OnHideNodeAutoCompleteBar(bool delayTransientDeletion = false)
        {
            if (ViewModel != null)
            {
                ViewModel.IsOpen = false;

                ViewModel.ParentNodeRemoved -= OnParentNodeRemoved;
                ViewModel.RefocusSearchBox -= OnRefocusSearchbox;
                Owner.LocationChanged -= OwnerMoved;
                ViewModel.PortViewModel.PortModel.Owner.PropertyChanged -= Owner_PropertyChanged;
                ViewModel.PortViewModel.NodeViewModel.WorkspaceViewModel.Model.PropertyChanged -= WorkspaceModel_PropertyChanged;
                
                ViewModel.PortViewModel.Highlight = Visibility.Hidden;
            }

            Hide();

            //if we're doing this while a node is being deleted, doing this synchronously will
            //cause an exception because of the current undo stack being open
            //TODO: Transient node operations shouldn't be recorded in the undo-redo stack
            if (delayTransientDeletion)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ViewModel?.DeleteTransientNodes();
                    ViewModel?.ToggleUndoRedoLocked(false);
                }), DispatcherPriority.Loaded);
            }
            else
            {
                ViewModel?.DeleteTransientNodes();
                ViewModel?.ToggleUndoRedoLocked(false);
            }
        }

        internal void OnShowNodeAutoCompleteBar()
        {
            //Analytics.TrackEvent(Actions.Open, Categories.NodeAutoCompleteOperations);

            if (ViewModel != null)
            {
                ViewModel.IsOpen = true;
                ViewModel.ParentNodeRemoved += OnParentNodeRemoved;
                ViewModel.RefocusSearchBox += OnRefocusSearchbox;
                Owner.LocationChanged += OwnerMoved;

                if (ViewModel.PortViewModel != null)
                {
                    ViewModel.PortViewModel.PortModel.Owner.PropertyChanged += Owner_PropertyChanged;
                    ViewModel.PortViewModel.NodeViewModel.WorkspaceViewModel.Model.PropertyChanged += WorkspaceModel_PropertyChanged;

                    ViewModel.PortViewModel.SetupNodeAutoCompleteClusterWindowPlacement(this);

                    ViewModel.PortViewModel.Highlight = Visibility.Visible;
                }

            }

            Show();

            // Call asynchronously to populate data when the window is ready.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ViewModel?.PopulateAutoComplete();
            }), DispatcherPriority.Loaded);
        }

        //Unsubscribe from events and destroy the node autocompletebar window.
        private void ResetNodeAutoCompleteBar()
        {
            UnsubscribeFromAppEvents();
            OnHideNodeAutoCompleteBar();
            Close();
            _controlInstance = null;
        }

        private void OnMainAppClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ResetNodeAutoCompleteBar();
        }

        private void OnWorkspaceClosed()
        {
            ResetNodeAutoCompleteBar();
        }

        void OnWorkspaceRemoved(WorkspaceModel workspace)
        {
            if (ViewModel?.PortViewModel?.NodeViewModel?.WorkspaceViewModel?.Model?.Guid == workspace?.Guid)
            {
                ResetNodeAutoCompleteBar();
            }
        }

        private void GripHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MoveIndex(int step)
        {
            ViewModel.SelectedIndex = Math.Min(ViewModel.FilteredView.Cast<object>().Count() - 1, Math.Max(0, ViewModel.SelectedIndex + step));
        }

        private void PrevButton_OnClick(object sender, RoutedEventArgs e)
        {
            MoveIndex(-1);
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            MoveIndex(+1);
        }

        private void DockButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ResultsLoaded)
            {
                ViewModel.PortViewModel.NodeViewModel.WorkspaceViewModel.OnRequestNodeAutoCompleteViewExtension(ViewModel.FullResults);
            }
        }

        //Removes nodeautocomplete menu when the associated parent node is removed.
        private void OnParentNodeRemoved(NodeModel node)
        {
            NodeModel parent_node = ViewModel.PortViewModel?.PortModel.Owner;
            if (ReferenceEquals(node, parent_node))
            {
                OnHideNodeAutoCompleteBar(true);
            }
        }

        private void OnAutoCompleteKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;

            switch (key)
            {
                case Key.Escape:
                    OnHideNodeAutoCompleteBar();
                    e.Handled = true;
                    break;
                case Key.Enter:
                    ViewModel?.ConsolidateTransientNodes();
                    OnHideNodeAutoCompleteBar();
                    e.Handled = true;
                    break;
                case Key.Up:
                case Key.Left:
                    MoveIndex(-1);
                    e.Handled = true;
                    break;
                case Key.Down:
                case Key.Right:
                    MoveIndex(+1);
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        internal void ConfirmAutoCompleteWindow(object sender, RoutedEventArgs e)
        {
            ViewModel?.ConsolidateTransientNodes();
            OnHideNodeAutoCompleteBar();
        }

        internal void CloseAutoCompleteWindow(object sender, RoutedEventArgs e)
        {
            OnHideNodeAutoCompleteBar();
        }

    }
}
