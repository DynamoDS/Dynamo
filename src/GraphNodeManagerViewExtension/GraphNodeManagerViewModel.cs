using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.GraphNodeManager.ViewModels;
using Dynamo.Wpf.Extensions;

namespace Dynamo.GraphNodeManager
{
    /// <summary>
    /// ViewModel for the Graph Node Manager
    /// Handles node table setup, workspace events, etc.
    /// Source: TuneUp https://github.com/DynamoDS/TuneUp/blob/master/TuneUp/TuneUpWindowViewModel.cs
    /// </summary>
    public class GraphNodeManagerViewModel : NotificationObject, IDisposable
    {
        #region Private Properties
        private readonly ViewLoadedParams viewLoadedParams;
        private HomeWorkspaceModel currentWorkspace;
        private Dictionary<Guid, NodeViewModel> nodeDictionary = new Dictionary<Guid, NodeViewModel>();
        private bool isEditingEnabled = true;
        private bool isRecomputeEnabled = true;

        private HomeWorkspaceModel CurrentWorkspace
        {
            get
            {
                return currentWorkspace;
            }
            set
            {
                // Unsubscribe from old workspace
                if (currentWorkspace != null)
                {
                    UnsubscribeWorkspaceEvents(currentWorkspace);
                }

                // Subscribe to new workspace
                if (value != null)
                {
                    // Set new workspace
                    currentWorkspace = value;
                    SubscribeWorkspaceEvents(currentWorkspace);
                }
            }
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Collection of data for nodes in the current workspace
        /// </summary>
        public ObservableCollection<NodeViewModel> Nodes { get; set; } = new ObservableCollection<NodeViewModel>();

        /// <summary>
        /// Collection of user filters
        /// </summary>
        public ObservableCollection<FilterViewModel> FilterItems { get; set; } = new ObservableCollection<FilterViewModel>();

        /// <summary>
        /// Collection of all current Workspace Nodes
        /// </summary>
        public CollectionViewSource NodesCollection { get; set; }


        /// <summary>
        /// Is the recomputeAll button enabled in the UI. Users should not be able to force a 
        /// reset of the engine and re-execution of the graph if one is still ongoing. This causes...trouble.
        /// Source: TuneUp https://github.com/DynamoDS/TuneUp
        /// </summary>
        public bool IsRecomputeEnabled
        {
            get => isRecomputeEnabled;
            private set
            {
                if (isRecomputeEnabled != value)
                {
                    isRecomputeEnabled = value;
                    RaisePropertyChanged(nameof(IsRecomputeEnabled));
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// Initialize the ViewLoadedParameters that contain most of the interesting stuff 
        /// Subscribe to the Workspace Changed and Cleared Events
        /// Establish the Current Workspace
        /// </summary>
        /// <param name="p"></param>
        public GraphNodeManagerViewModel(ViewLoadedParams p)
        {
            this.viewLoadedParams = p;

            p.CurrentWorkspaceChanged += OnCurrentWorkspaceChanged;
            p.CurrentWorkspaceCleared += OnCurrentWorkspaceCleared;
            
            if (p.CurrentWorkspaceModel is HomeWorkspaceModel)
            {
                CurrentWorkspace = p.CurrentWorkspaceModel as HomeWorkspaceModel;
            }

            InitializeFilters();
        }

        private void InitializeFilters()
        {
            FilterItems.Add(new FilterViewModel(){Name = "Empty List"});
            FilterItems.Add(new FilterViewModel(){Name = "Error"});
            FilterItems.Add(new FilterViewModel(){Name = "Frozen"});
            FilterItems.Add(new FilterViewModel(){Name = "Function"});
            FilterItems.Add(new FilterViewModel(){Name = "Information"});
            FilterItems.Add(new FilterViewModel(){Name = "Is Input"});
            FilterItems.Add(new FilterViewModel(){Name = "Is Output"});
            FilterItems.Add(new FilterViewModel(){Name = "Null"});
            FilterItems.Add(new FilterViewModel(){Name = "Warning"});
            FilterItems.Add(new FilterViewModel(){Name = "Preview off"});
        }

        #endregion

        #region Node Methods
        /// <summary>
        /// The main method to set the Node Collection
        /// Will be triggered in two occasions:
        /// - a new workspace is established
        /// - on Run (or after Run has finished?)
        /// </summary>
        private void ResetNodes()
        {
            if (CurrentWorkspace == null)
            {
                return;
            }
            nodeDictionary.Clear();
            Nodes.Clear();
            foreach (var node in CurrentWorkspace.Nodes)
            {
                var profiledNode = new NodeViewModel(node);
                nodeDictionary[node.GUID] = profiledNode;
                Nodes.Add(profiledNode);
            }

            NodesCollection = new CollectionViewSource();
            NodesCollection.Source = Nodes;
            // Sort the data by execution state
            //NodesCollection.GroupDescriptions.Add(new PropertyGroupDescription(nameof(NodeViewModel.StateDescription)));
            //NodesCollection.SortDescriptions.Add(new SortDescription(nameof(NodeViewModel.State), ListSortDirection.Ascending));
            NodesCollection.View?.Refresh();

            RaisePropertyChanged(nameof(NodesCollection));
            RaisePropertyChanged(nameof(Nodes));
        }

        /// <summary>
        /// Enable editing when it is disabled temporarily.
        /// </summary>
        internal void EnableEditing()
        {
            if (!isEditingEnabled && CurrentWorkspace != null)
            {
                ResetNodes();
                CurrentWorkspace.EngineController.EnableProfiling(true, CurrentWorkspace, CurrentWorkspace.Nodes);
                isEditingEnabled = true;
            }
            RaisePropertyChanged(nameof(NodesCollection));
        }

        #endregion

        #region Workspace EventHandlers

        /// <summary>
        /// On evaluation started lock  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentWorkspaceModel_EvaluationStarted(object sender, EventArgs e)
        {
            IsRecomputeEnabled = false;

            EnableEditing();
        }

        /// <summary>
        /// On evaluation finished unlock 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentWorkspaceModel_EvaluationCompleted(object sender, EventArgs e)
        {
            IsRecomputeEnabled = true;

            RaisePropertyChanged(nameof(NodesCollection));
            RaisePropertyChanged(nameof(Nodes));

            NodesCollection.Dispatcher.Invoke(() =>
            {
                NodesCollection.SortDescriptions.Clear();
                // Sort nodes into execution group
                //NodesCollection.SortDescriptions.Add(new SortDescription(nameof(NodeViewModel.State), ListSortDirection.Ascending));
                
                if (NodesCollection.View != null)
                    NodesCollection.View.Refresh();
            });
        }

        /// <summary>
        /// When a new node is added, update the collection
        /// </summary>
        /// <param name="node"></param>
        private void CurrentWorkspaceModel_NodeAdded(NodeModel node)
        {
            var infoNode = new NodeViewModel(node);
            nodeDictionary[node.GUID] = infoNode;
            Nodes.Add(infoNode);
            RaisePropertyChanged(nameof(NodesCollection));
        }
        /// <summary>
        /// When a node is removed, update the collection
        /// </summary>
        /// <param name="node"></param>
        private void CurrentWorkspaceModel_NodeRemoved(NodeModel node)
        {
            var infoNode = nodeDictionary[node.GUID];
            nodeDictionary.Remove(node.GUID);
            Nodes.Remove(infoNode);
            RaisePropertyChanged(nameof(NodesCollection));
        }

        private void OnCurrentWorkspaceCleared(IWorkspaceModel workspace)
        {
            // Editing needs to be enabled per workspace so mark it false after closing
            isEditingEnabled = false;
            CurrentWorkspace = viewLoadedParams.CurrentWorkspaceModel as HomeWorkspaceModel;
        }

        private void OnCurrentWorkspaceChanged(IWorkspaceModel workspace)
        {
            // Editing needs to be enabled per workspace so mark it false after switching
            isEditingEnabled = false;
            CurrentWorkspace = workspace as HomeWorkspaceModel;
        }

        #endregion

        #region Setup and Dispose Methods

        /// <summary>
        /// When a new workspace is established
        /// </summary>
        /// <param name="workspace"></param>
        private void SubscribeWorkspaceEvents(HomeWorkspaceModel workspace)
        {
            workspace.NodeAdded += CurrentWorkspaceModel_NodeAdded;
            workspace.NodeRemoved += CurrentWorkspaceModel_NodeRemoved;
            workspace.EvaluationStarted += CurrentWorkspaceModel_EvaluationStarted;
            workspace.EvaluationCompleted += CurrentWorkspaceModel_EvaluationCompleted;

            ResetNodes();
        }

        /// <summary>
        /// When we change workspace
        /// </summary>
        /// <param name="workspace"></param>
        private void UnsubscribeWorkspaceEvents(HomeWorkspaceModel workspace)
        {
            workspace.NodeAdded -= CurrentWorkspaceModel_NodeAdded;
            workspace.NodeRemoved -= CurrentWorkspaceModel_NodeRemoved;
            workspace.EvaluationStarted -= CurrentWorkspaceModel_EvaluationStarted;
            workspace.EvaluationCompleted -= CurrentWorkspaceModel_EvaluationCompleted;
        }

        /// <summary>
        /// ViewModel dispose method
        /// </summary>
        public void Dispose()
        {
            UnsubscribeWorkspaceEvents(CurrentWorkspace);
            viewLoadedParams.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;
            viewLoadedParams.CurrentWorkspaceCleared -= OnCurrentWorkspaceCleared;
        }

        #endregion
    }
}
