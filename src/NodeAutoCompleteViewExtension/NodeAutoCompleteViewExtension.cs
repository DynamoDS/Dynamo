using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Extensions;
using Dynamo.Logging;
using Dynamo.Graph.Workspaces;
using Dynamo.NodeAutoComplete.ViewModels;
using Dynamo.NodeAutoComplete.Views;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;
using Dynamo.Views;
using Dynamo.Wpf.Extensions;

namespace Dynamo.NodeAutoComplete
{
    /// <summary>
    /// This view extension tracks current clicked node port in Dynamo and 
    /// tries to suggest the next best node to connect that port.
    /// </summary>
    public class NodeAutoCompleteViewExtension : ViewExtensionBase, IViewExtension, ILogSource
    {
        private const String extensionName = "Node Auto Complete";
        private ViewLoadedParams viewLoadedParamsReference;
        private NodeAutoCompletePanelViewModel nodeAutoCompleteViewModel;

        internal MenuItem nodeAutocompleteMenuItem;

        /// <summary>
        /// Internal cache of the data displayed in data grid, useful in unit testing.
        /// You are not expected to modify this but rather inspection.
        /// </summary>
        internal IEnumerable<NodeAutocompleteCluster> nodeAutocompleteClusters;

        

        internal NodeAutoCompleteView DependencyView
        {
            get;
            set;
        }

        /// <summary>
        /// Extension Name
        /// </summary>
        public override string Name
        {
            get
            {
                return extensionName;
            }
        }

        /// <summary>
        /// GUID of the extension
        /// </summary>
        public override string UniqueId
        {
            get
            {
                return "64F28473-0DCB-4E41-BB5B-A409FF6C90AD";
            }
        }

        /// <summary>
        /// Dispose function after extension is closed
        /// </summary>
        public override void Dispose()
        {
            DependencyView?.Dispose();
            nodeAutocompleteClusters = null;
        }

        public void Ready(ReadyParams readyParams)
        {
            // Do nothing for now
        }

        public override void Shutdown()
        {
            WorkspaceView.RequesNodeAutoCompleteBar -= OnNodeAutoCompleteBarRequested;
        }

        public override void Startup(ViewStartupParams viewStartupParams)
        {
            // Do nothing for now

        }

        public event Action<ILogMessage> MessageLogged;

        internal void OnMessageLogged(ILogMessage msg)
        {
            this.MessageLogged?.Invoke(msg);
        }

        internal void AddToSidebar()
        {
            // Dont allow the extension to show in anything that isnt a HomeWorkspaceModel
            if (!(this.viewLoadedParamsReference.CurrentWorkspaceModel is HomeWorkspaceModel))
            {
                this.Closed();
                return;
            }

            this.viewLoadedParamsReference?.AddToExtensionsSideBar(this, DependencyView);
        }

        internal void ShowViewExtension()
        {
            AddToSidebar();
            this.nodeAutocompleteMenuItem.IsChecked = true;
        }

        public override void Loaded(ViewLoadedParams viewLoadedParams)
        {
            this.viewLoadedParamsReference = viewLoadedParams ?? throw new ArgumentNullException(nameof(viewLoadedParams));
            var dynamoViewModel = viewLoadedParams.DynamoWindow.DataContext as DynamoViewModel;

            if (dynamoViewModel.IsDNAClusterPlacementEnabled)
            {
                DependencyView = new NodeAutoCompleteView(this, viewLoadedParams);
                nodeAutoCompleteViewModel = new NodeAutoCompletePanelViewModel(viewLoadedParams.DynamoWindow, dynamoViewModel);

                // Adding a button in view menu to refresh and show manually
                nodeAutocompleteMenuItem = new MenuItem { Header = "Show NodeAutocomplete view extension", IsCheckable = true, IsChecked = false };
                nodeAutocompleteMenuItem.Click += (sender, args) =>
                {
                    if (nodeAutocompleteMenuItem.IsChecked)
                    {
                        viewLoadedParams.AddToExtensionsSideBar(this, DependencyView);
                        nodeAutocompleteMenuItem.IsChecked = true;
                    }
                    else
                    {
                        viewLoadedParams.CloseExtensioninInSideBar(this);
                        nodeAutocompleteMenuItem.IsChecked = false;
                    }
                };
                viewLoadedParams.AddExtensionMenuItem(nodeAutocompleteMenuItem);
            }

            WorkspaceView.RequesNodeAutoCompleteBar += OnNodeAutoCompleteBarRequested;

        }

        public override void Closed()
        {
            if (this.nodeAutocompleteMenuItem != null)
            {
                this.nodeAutocompleteMenuItem.IsChecked = false;
            }
        }

        internal void ShowClusterNodeAutocompleteResults(MLNodeClusterAutoCompletionResponse results)
        {
            nodeAutocompleteClusters = results.Results.ToList().Select(r => new NodeAutocompleteCluster(r));

            DependencyView.MainItems.ItemsSource = nodeAutocompleteClusters;
        }

        private static NodeAutoCompleteBarViewModel nodeAutoCompleteBarViewModel;

        private void OnNodeAutoCompleteBarRequested(Window parentWindow, ViewModelBase viewModelBase)
        {
            PortViewModel portViewModel = viewModelBase as PortViewModel;
            if (parentWindow is null || portViewModel is null)
            {
                return;
            }
            
            if (nodeAutoCompleteBarViewModel is null)
            {
                DynamoViewModel dynamoViewModel = portViewModel?.NodeViewModel?.WorkspaceViewModel?.DynamoViewModel;
                nodeAutoCompleteBarViewModel = new NodeAutoCompleteBarViewModel(dynamoViewModel);
            }

            if(nodeAutoCompleteBarViewModel.IsOpen)
            {
                return;
            }

            if (nodeAutoCompleteBarViewModel.PortViewModel != null)
            {
                nodeAutoCompleteBarViewModel.PortViewModel.Highlight = Visibility.Collapsed;
            }

            nodeAutoCompleteBarViewModel.PortViewModel = portViewModel;

            //TODO : I think we can reuse the window too.
            var nodeAutoCompleteBarWindow = new NodeAutoCompleteBarView(parentWindow, nodeAutoCompleteBarViewModel);
            nodeAutoCompleteBarWindow.Show();
            portViewModel.SetupNodeAutoCompleteClusterWindowPlacement(nodeAutoCompleteBarWindow);
        }
    }
}
