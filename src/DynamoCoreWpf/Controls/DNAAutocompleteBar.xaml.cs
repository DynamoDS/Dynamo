using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        ListBoxItem HighlightedItem;

        internal event Action<ShowHideFlags> RequestShowNodeAutoCompleteSearch;

        double currentX;

        ListBoxItem currentListBoxItem;

        /// <summary>
        /// Node AutoComplete Search ViewModel DataContext
        /// </summary>
        public NodeAutoCompleteSearchViewModel ViewModel => DataContext as NodeAutoCompleteSearchViewModel;

        public DNAAutocompleteBar()
        {
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
            OnRequestShowNodeAutoCompleteSearch(ShowHideFlags.Hide);
        }

        private void OnRequestShowNodeAutoCompleteSearch(ShowHideFlags flags)
        {
            RequestShowNodeAutoCompleteSearch?.Invoke(flags);
        }

        private void OnNodeAutoCompleteSearchControlVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // If visibility  is false, then stop processing it.
            if (!(bool)e.NewValue)
                return;

            // When launching this control, always start with clear search term.
            //SearchTextBox.Clear(); TODO

            Analytics.TrackEvent(
            Dynamo.Logging.Actions.Open,
            Dynamo.Logging.Categories.NodeAutoCompleteOperations);
            ViewModel.ClusterResults = null;

            // Visibility of textbox changed, but text box has not been initialized(rendered) yet.
            // Call asynchronously focus, when textbox will be ready.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                //SearchTextBox.Focus(); TODO
                ViewModel.PopulateClusterAutoComplete();
                //ViewModel.PopulateAutoCompleteCandidates();
            }), DispatcherPriority.Loaded);

            ViewModel.ParentNodeRemoved += OnParentNodeRemoved;
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
                OnRequestShowNodeAutoCompleteSearch(ShowHideFlags.Hide);
                ViewModel.ParentNodeRemoved -= OnParentNodeRemoved;
            }
        }

        private void OnInCanvasSearchKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;

            switch (key)
            {
                case Key.Escape:
                    OnRequestShowNodeAutoCompleteSearch(ShowHideFlags.Hide);
                    break;
                case Key.Enter:
                    if (HighlightedItem != null && ViewModel.CurrentMode != SearchViewModel.ViewMode.LibraryView)
                    {
                        //TODO: consolidate transient nodes
                        OnRequestShowNodeAutoCompleteSearch(ShowHideFlags.Hide);
                    }
                    break;
            }
        }

        internal void CloseAutocompletionWindow(object sender, RoutedEventArgs e)
        {
            CloseAutoCompletion();
        }

        internal void CloseAutoCompletion()
        {
            OnRequestShowNodeAutoCompleteSearch(ShowHideFlags.Hide);
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
