using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for AutoCompleteSearchControl.xaml
    /// Notice this control shares a lot of logic with InCanvasSearchControl for now
    /// But they will diverge eventually because of UI improvements to auto complete.
    /// </summary>
    public partial class DNAAutocompleteBar : IDisposable
    {
        /// <summary>
        /// Node AutoComplete Search ViewModel DataContext
        /// </summary>
        private NodeAutoCompleteSearchViewModel ViewModel => DataContext as NodeAutoCompleteSearchViewModel;

        public DNAAutocompleteBar(Window window, NodeAutoCompleteSearchViewModel viewModel)
        {
            viewModel.PortViewModel.Highlight = Visibility.Visible;
            this.Owner = window;
            DataContext = viewModel;
            InitializeComponent();
            if (string.IsNullOrEmpty(DynamoModel.HostAnalyticsInfo.HostName) && Application.Current != null)
            {
                Application.Current.Deactivated += CurrentApplicationDeactivated;
                if (Application.Current?.MainWindow != null)
                {
                    Application.Current.MainWindow.Closing += NodeAutoCompleteSearchControl_Unloaded;
                }
            }
            HomeWorkspaceModel.WorkspaceClosed += this.CloseAutoCompletion;
            ViewModel.IsOpen = true;
            LoadAndPopulate();
        }

        private void NodeAutoCompleteSearchControl_Unloaded(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(DynamoModel.HostAnalyticsInfo.HostName) && Application.Current != null)
            {
                Application.Current.Deactivated -= CurrentApplicationDeactivated;
                if (Application.Current?.MainWindow != null)
                {
                    Application.Current.MainWindow.Closing -= NodeAutoCompleteSearchControl_Unloaded;
                }
            }
            HomeWorkspaceModel.WorkspaceClosed -= this.CloseAutoCompletion;
        }

        private void CurrentApplicationDeactivated(object sender, EventArgs e)
        {
            CloseAutoCompletion();
        }

        private void LoadAndPopulate()
        {
            Analytics.TrackEvent(
            Dynamo.Logging.Actions.Open,
            Dynamo.Logging.Categories.NodeAutoCompleteOperations);
            ViewModel.ClusterResults = null;

            // Visibility of textbox changed, but text box has not been initialized(rendered) yet.
            // Call asynchronously focus, when textbox will be ready.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ViewModel.PopulateClusterAutoComplete();
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
            ViewModel.SelectedIndex = Math.Min(ViewModel.ClusterResults.Count() - 1, Math.Max(0, ViewModel.SelectedIndex + step));
        }

        private void PrevButton_OnClick(object sender, RoutedEventArgs e)
        {
            MoveIndex(-1);
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            MoveIndex(+1);
        }

        //Removes nodeautocomplete menu when the associated parent node is removed.
        private void OnParentNodeRemoved(NodeModel node)
        {
            NodeModel parent_node = ViewModel.PortViewModel?.PortModel.Owner;
            if (node == parent_node)
            {
                CloseAutoCompletion();
                ViewModel.ParentNodeRemoved -= OnParentNodeRemoved;
            }
        }

        private void OnDNAAutocompleteKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;

            switch (key)
            {
                case Key.Escape:
                    CloseAutoCompletion();
                    break;
                case Key.Enter:
                    ViewModel?.ConsolidateTransientNodes();
                    CloseAutoCompletion();
                    break;
            }
        }

        internal void ConfirmAutocompletionWindow(object sender, RoutedEventArgs e)
        {
            ViewModel?.ConsolidateTransientNodes();
            CloseAutoCompletion();
        }

        internal void CloseAutocompletionWindow(object sender, RoutedEventArgs e)
        {
            CloseAutoCompletion();
        }

        internal void CloseAutoCompletion()
        {
            //if we're doing this while a node is being deleted, doing this synchronously will
            //cause an exception because of the current undo stack being open
            //TODO: Transient node operations shouldn't be recorded in the undo-redo stack
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ViewModel?.DeleteTransientNodes();
            }), DispatcherPriority.Loaded);
            ViewModel.PortViewModel.Highlight = Visibility.Hidden;
            ViewModel.IsOpen = false;
            this.Close();
            ViewModel?.OnNodeAutoCompleteWindowClosed();
        }

        /// <summary>
        /// Dispose the control
        /// </summary>
        public void Dispose()
        {
            NodeAutoCompleteSearchControl_Unloaded(this, null);
        }
    }
}
