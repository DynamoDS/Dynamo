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
using Dynamo.ViewModels;

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
            SubscribeEvents();
            LoadAndPopulate();
        }

        internal void ReloadDataContext(NodeAutoCompleteBarViewModel dataContext)
        {
            DataContext = dataContext;
            UnsubscribeEvents(this, null);
            SubscribeEvents();
            LoadAndPopulate();
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

        private void SubscribeEvents()
        {
            if (string.IsNullOrEmpty(DynamoModel.HostAnalyticsInfo.HostName) && Application.Current != null)
            {
                if (Application.Current?.MainWindow != null)
                {
                    Application.Current.MainWindow.Closing += UnsubscribeEvents;
                }
            }
            HomeWorkspaceModel.WorkspaceClosed += CloseAutoComplete;
            ViewModel.ParentNodeRemoved += OnParentNodeRemoved;
            ViewModel.RefocusSearchBox += OnRefocusSearchbox;
            Owner.LocationChanged += OwnerMoved;
            ViewModel.PortViewModel.PortModel.Owner.PropertyChanged += Owner_PropertyChanged;
            ViewModel.PortViewModel.NodeViewModel.WorkspaceViewModel.Model.PropertyChanged += WorkspaceModel_PropertyChanged;
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
            ViewModel.ParentNodeRemoved -= OnParentNodeRemoved;
            ViewModel.RefocusSearchBox -= OnRefocusSearchbox;
            Owner.LocationChanged -= OwnerMoved;
            ViewModel.PortViewModel.PortModel.Owner.PropertyChanged -= Owner_PropertyChanged;
            ViewModel.PortViewModel.NodeViewModel.WorkspaceViewModel.Model.PropertyChanged -= WorkspaceModel_PropertyChanged;
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

        private void LoadAndPopulate()
        {
            ViewModel.IsOpen = true;
            Analytics.TrackEvent(Actions.Open, Categories.NodeAutoCompleteOperations);

            ViewModel.PopulateAutoComplete();
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
            if (node == parent_node)
            {
                CloseAutoComplete();
            }
        }

        private void OnAutoCompleteKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;

            switch (key)
            {
                case Key.Escape:
                    CloseAutoComplete();
                    e.Handled = true;
                    break;
                case Key.Enter:
                    ViewModel?.ConsolidateTransientNodes();
                    CloseAutoComplete();
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
            CloseAutoComplete();
        }

        internal void ImageAI_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.dynamoViewModel.OpenDocumentationLinkCommand.Execute(new OpenDocumentationLinkEventArgs(new Uri(Wpf.Properties.Resources.NodeAutocompleteDocumentationUriString, UriKind.Relative)));
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
            }), DispatcherPriority.Loaded);

            Close();
            UnsubscribeEvents(this, null);
        }

    }
}
