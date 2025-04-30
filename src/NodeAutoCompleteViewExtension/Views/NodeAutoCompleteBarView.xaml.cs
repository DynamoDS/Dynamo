using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.NodeAutoComplete.ViewModels;
using Dynamo.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Dynamo.NodeAutoComplete.Views
{
    /// <summary>
    /// Interaction logic for NodeAutoCompleteBarView.xaml
    /// </summary>
    public partial class NodeAutoCompleteBarView
    {
        private NodeAutoCompleteBarViewModel ViewModel => DataContext as NodeAutoCompleteBarViewModel;

        public NodeAutoCompleteBarView(Window window, NodeAutoCompleteBarViewModel viewModel)
        {            
            Owner = window;
            DataContext = viewModel;
            InitializeComponent();

            viewModel.PortViewModel.Highlight = Visibility.Visible;

            if (string.IsNullOrEmpty(DynamoModel.HostAnalyticsInfo.HostName) && Application.Current != null)
            {
                if (Application.Current?.MainWindow != null)
                {
                    Application.Current.MainWindow.Closing += UnsubscribeEvents;
                }
            }
            HomeWorkspaceModel.WorkspaceClosed += CloseAutoComplete;
            viewModel.IsOpen = true;
            LoadAndPopulate();
        }

        private void UnsubscribeEvents(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(DynamoModel.HostAnalyticsInfo.HostName) && Application.Current != null)
            {
                if (Application.Current?.MainWindow != null)
                {
                    Application.Current.MainWindow.Closing -= UnsubscribeEvents;
                }
            }
            HomeWorkspaceModel.WorkspaceClosed -= CloseAutoComplete;
        }

        private void LoadAndPopulate()
        {
            Analytics.TrackEvent(
            Dynamo.Logging.Actions.Open,
            Dynamo.Logging.Categories.NodeAutoCompleteOperations);
            ViewModel.DropdownResults = null;

            // Visibility of textbox changed, but text box has not been initialized(rendered) yet.
            // Call asynchronously focus, when textbox will be ready.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ViewModel.PopulateAutoComplete();
                //ViewModel.PopulateAutoCompleteCandidates();
            }), DispatcherPriority.Loaded);

            ViewModel.ParentNodeRemoved += OnParentNodeRemoved;
        }

        private void GripHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MoveIndex(int step)
        {
            ViewModel.SelectedIndex = Math.Min(ViewModel.DropdownResults.Count() - 1, Math.Max(0, ViewModel.SelectedIndex + step));
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
            if (node == parent_node)
            {
                CloseAutoComplete();
                ViewModel.ParentNodeRemoved -= OnParentNodeRemoved;
            }
        }

        private void OnAutoCompleteKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;

            switch (key)
            {
                case Key.Escape:
                    CloseAutoComplete();
                    break;
                case Key.Enter:
                    ViewModel?.ConsolidateTransientNodes();
                    CloseAutoComplete();
                    break;
            }
        }

        internal void ConfirmAutoCompleteWindow(object sender, RoutedEventArgs e)
        {
            ViewModel?.ConsolidateTransientNodes();
            CloseAutoComplete();
        }

        internal void CloseAutoCompleteWindow(object sender, RoutedEventArgs e)
        {
            CloseAutoComplete();
        }

        internal void CloseAutoComplete()
        {
            ViewModel.IsOpen = false;
            ViewModel.PortViewModel.Highlight = Visibility.Hidden;

            //if we're doing this while a node is being deleted, doing this synchronously will
            //cause an exception because of the current undo stack being open
            //TODO: Transient node operations shouldn't be recorded in the undo-redo stack
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ViewModel?.DeleteTransientNodes();
                ViewModel?.ToggleUndoRedoLocked(false);
            }), DispatcherPriority.Loaded);
            
            Close();
            UnsubscribeEvents(this, null);
        }

    }
}
