using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.NodeAutoComplete;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.Utilities;


namespace Dynamo.NodeAutoComplete
{
    /// <summary>
    /// Interaction logic for WorkspaceDependencyView.xaml
    /// </summary>
    public partial class NodeAutoCompleteView : UserControl, IDisposable
    {

        internal WorkspaceModel currentWorkspace;

        /// <summary>
        /// The hyper link where Dynamo user will be forwarded to for submitting comments.
        /// </summary>
        private readonly string FeedbackLink = "https://forum.dynamobim.com/t/call-for-feedback-on-dynamo-graph-package-dependency-display/37229";

        internal ViewLoadedParams loadedParams;
        private readonly NodeAutoCompleteViewExtension nodeAutocompleteViewExtension;


        internal CustomNodeManager CustomNodeManager { get; set; }

        /// <summary>
        /// Event handler for workspaceAdded event
        /// </summary>
        /// <param name="obj">workspace model</param>
        internal void OnWorkspaceChanged(IWorkspaceModel obj)
        {
            if (obj is WorkspaceModel)
            {
                // Unsubscribe
                if (currentWorkspace != null)
                {
                    currentWorkspace.PropertyChanged -= OnWorkspacePropertyChanged;
                }

                // Update current workspace
                currentWorkspace = obj as WorkspaceModel;
                currentWorkspace.PropertyChanged += OnWorkspacePropertyChanged;
            }
        }

        /// <summary>
        /// Event handler for workspaceRemoved event
        /// </summary>
        /// <param name="obj">workspace model</param>
        internal void OnWorkspaceCleared(IWorkspaceModel obj)
        {
            if (obj is WorkspaceModel)
            {
               //
            }
        }

        private void OnWorkspacePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            //
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">ViewLoadedParams</param>
        public NodeAutoCompleteView(NodeAutoCompleteViewExtension viewExtension, ViewLoadedParams p)
        {
            InitializeComponent();
            this.DataContext = this;
            currentWorkspace = p.CurrentWorkspaceModel as WorkspaceModel;
            p.CurrentWorkspaceChanged += OnWorkspaceChanged;
            p.CurrentWorkspaceCleared += OnWorkspaceCleared;
            currentWorkspace.PropertyChanged += OnWorkspacePropertyChanged;
            loadedParams = p;
            nodeAutocompleteViewExtension = viewExtension;
            WorkspaceViewModel.RequestNodeAutoCompleteViewExtension += ShowNodeAutoCompleteViewExtension;
            HomeWorkspaceModel.WorkspaceClosed += this.CloseExtensionTab;
        }

        /// <summary>
        /// Calls DependencyRegen with forceCompute as true, as dummy nodes are reloaded.
        /// </summary>
        internal void ShowNodeAutoCompleteViewExtension(MLNodeClusterAutoCompletionResponse results)
        {
            nodeAutocompleteViewExtension.ShowViewExtension();
            nodeAutocompleteViewExtension.ShowClusterNodeAutocompleteResults(results);
        }

        /// <summary>
        /// This method will call the close API on the workspace references extension. 
        /// </summary>
        internal void CloseExtensionTab()
        {
            loadedParams.CloseExtensioninInSideBar(nodeAutocompleteViewExtension);
        }


        /// <summary>
        /// Dispose function for WorkspaceDependencyView
        /// </summary>
        public void Dispose()
        {
            loadedParams.CurrentWorkspaceChanged -= OnWorkspaceChanged;
            loadedParams.CurrentWorkspaceCleared -= OnWorkspaceCleared;
            currentWorkspace.PropertyChanged -= OnWorkspacePropertyChanged;
            WorkspaceViewModel.RequestNodeAutoCompleteViewExtension -= ShowNodeAutoCompleteViewExtension;
            HomeWorkspaceModel.WorkspaceClosed -= this.CloseExtensionTab;
        }

        private void Refresh_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //
        }

        private void ForceRefresh_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //
        }
    }


    /// <summary>
    /// Represents information about a package dependency as a row in the dependency table
    /// </summary>
    public class NodeAutocompleteCluster
    {
        private readonly ClusterResultItem clusterResultItem;

        internal NodeAutocompleteCluster(ClusterResultItem result)
        {
            clusterResultItem = result;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Title => clusterResultItem.Title;

        /// <summary>
        /// Node autocomplete cluster results description
        /// </summary>
        public string Description => clusterResultItem.Description;

        /// <summary>
        /// 
        /// </summary>
        public int EntryNodeIndex => clusterResultItem.EntryNodeIndex;

        /// <summary>
        /// 
        /// </summary>
        public int EntryNodeInPort => clusterResultItem.EntryNodeInPort;


        /// <summary>
        ///
        /// </summary>
        public string Probability => clusterResultItem.Probability;

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<NodeItem> Nodes => clusterResultItem.Topology.Nodes;

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<ConnectionItem> Connections => clusterResultItem.Topology.Connections;
    }
}
