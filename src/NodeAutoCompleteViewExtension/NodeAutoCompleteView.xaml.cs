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

        internal ViewLoadedParams loadedParams;
        private readonly NodeAutoCompleteViewExtension nodeAutocompleteViewExtension;

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
               // do nothing for now
            }
        }

        private void OnWorkspacePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            //  do nothing for now
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
    }


    /// <summary>
    /// Represents information about a node cluster suggestion in the view extension.
    /// </summary>
    public class NodeAutocompleteCluster
    {
        private readonly ClusterResultItem clusterResultItem;

        internal NodeAutocompleteCluster(ClusterResultItem result)
        {
            clusterResultItem = result;
        }

        /// <summary>
        /// Title of the node cluster 
        /// </summary>
        public string Title => clusterResultItem.Title;

        /// <summary>
        /// Description of the node cluster 
        /// </summary>
        public string Description => clusterResultItem.Description;

        /// <summary>
        /// Entry node index for this node cluster
        /// </summary>
        public int EntryNodeIndex => clusterResultItem.EntryNodeIndex;

        /// <summary>
        /// Entry node inport for this node cluster
        /// </summary>
        public int EntryNodeInPort => clusterResultItem.EntryNodeInPort;

        /// <summary>
        /// confidence level
        /// </summary>
        public string Probability => clusterResultItem.Probability;

        /// <summary>
        /// Nodes in the node cluster
        /// </summary>
        internal IEnumerable<NodeItem> Nodes => clusterResultItem.Topology.Nodes;

        /// <summary>
        /// Connections in the node cluster
        /// </summary>
        internal IEnumerable<ConnectionItem> Connections => clusterResultItem.Topology.Connections;
    }
}
