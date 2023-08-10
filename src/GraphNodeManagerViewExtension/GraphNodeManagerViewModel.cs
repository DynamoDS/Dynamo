using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
//using System.Windows.Input;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.GraphNodeManager.ViewModels;
using Dynamo.Models;
using Dynamo.Wpf.Extensions;
using Dynamo.Extensions;
using Dynamo.GraphNodeManager.Properties;
using Dynamo.PackageManager;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Prism.Commands;
using DelegateCommand = Dynamo.UI.Commands.DelegateCommand;
using GridNodeViewModel = Dynamo.GraphNodeManager.ViewModels.GridNodeViewModel;
using Newtonsoft.Json;

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

        private readonly ViewModelCommandExecutive viewModelCommandExecutive;

        private readonly string uniqueId;

        private readonly Dictionary<Guid, GridNodeViewModel> nodeDictionary = new Dictionary<Guid, GridNodeViewModel>();
        private Dictionary<string, FilterViewModel> filterDictionary = new Dictionary<string, FilterViewModel>();

        private bool isEditingEnabled = true;
        private bool isAnyFilterOn = false;
        private Action<Logging.ILogMessage> logMessage;

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

        private ViewModelCommandExecutive viewModelExecutive;
        private readonly ICommandExecutive commandExecutive;

        #endregion

        #region Public Properties
        /// <summary>
        /// Collection of data for nodes in the current workspace
        /// </summary>
        public ObservableCollection<GridNodeViewModel> Nodes { get; set; } = new ObservableCollection<GridNodeViewModel>();

        /// <summary>
        /// Collection of user filters
        /// </summary>
        public ObservableCollection<FilterViewModel> FilterItems { get; set; } = new ObservableCollection<FilterViewModel>();

        /// <summary>
        /// Collection of all current Workspace Nodes
        /// </summary>
        public CollectionViewSource NodesCollection { get; set; }

        public DelegateCommand NodeSelectCommand { get; set; }
        public DelegateCommand ClearFiltersCommand { get; set; }
        public DelegateCommand<string> ExportCommand { get; set; }


        public string searchText;
        /// <summary>
        /// Search Box Text binding
        /// </summary>
        [JsonIgnore]
        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;

                // Every time we type in the Search Box, we will be updating the Filter
                NodesCollectionFilter_Changed();    
                RaisePropertyChanged(nameof(SearchText));
            }
        }

        public GraphNodeManagerView GraphNodeManagerView;

        public string searchBoxPrompt = "Search..";
        /// <summary>
        /// Search Box Prompt binding
        /// </summary>
        [JsonIgnore]
        public string SearchBoxPrompt
        {
            get { return searchBoxPrompt; }
            set
            {
                searchBoxPrompt = value;
                RaisePropertyChanged(nameof(SearchBoxPrompt));
            }
        }

        private bool isRecomputeEnabled = true;
        /// <summary>
        /// Is the recomputeAll button enabled in the UI. Users should not be able to force a 
        /// reset of the engine and re-execution of the graph if one is still ongoing. This causes...trouble.
        /// Source: TuneUp https://github.com/DynamoDS/TuneUp
        /// </summary>
        [JsonIgnore]
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

        internal string DynamoVersion;
        internal string HostName;

        [JsonIgnore]
        public bool IsAnyFilterOn
        {
            get
            {
                return FilterItems.Any(f => f.IsFilterOn);
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
        public GraphNodeManagerViewModel(ViewLoadedParams p, string id, Action<Logging.ILogMessage> logDelegate)
        {
            this.viewLoadedParams = p;
            logMessage = logDelegate;

            p.CurrentWorkspaceChanged += OnCurrentWorkspaceChanged;
            p.CurrentWorkspaceCleared += OnCurrentWorkspaceCleared;
            
            if (p.CurrentWorkspaceModel is HomeWorkspaceModel)
            {
                CurrentWorkspace = p.CurrentWorkspaceModel as HomeWorkspaceModel;
                viewModelExecutive = p.ViewModelCommandExecutive;
                commandExecutive = p.CommandExecutive;
                viewModelCommandExecutive = p.ViewModelCommandExecutive;
                uniqueId = id;
            }

            InitializeFilters();

            NodeSelectCommand = new DelegateCommand(NodeSelect);
            ClearFiltersCommand = new DelegateCommand(ClearAllFilters);
            ExportCommand = new DelegateCommand<string>(ExportGraph);

            DynamoVersion = p.StartupParams.DynamoVersion.ToString();

            var dynamoViewModel = p.DynamoWindow.DataContext as DynamoViewModel;
            HostName = dynamoViewModel.Model.HostName;  // will become obsolete in Dynamo 3.0

            // For node package info
            var pmExtension = viewLoadedParams.ViewStartupParams.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();

        }


        private void InitializeFilters()
        {   
            FilterItems.Add(new FilterViewModel(this){Name = Resources.Title_EmptyList, FilterImage = ResourceUtilities.ConvertToImageSource(Properties.Resources.EmptyList)  }); 
            FilterItems.Add(new FilterViewModel(this){Name = Resources.Title_Error, FilterImage = ResourceUtilities.ConvertToImageSource(Properties.Resources.Error) });
            FilterItems.Add(new FilterViewModel(this){Name = Resources.Title_Frozen, FilterImage = ResourceUtilities.ConvertToImageSource(Properties.Resources.Frozen) });
            FilterItems.Add(new FilterViewModel(this){Name = Resources.Title_Function, FilterImage = ResourceUtilities.ConvertToImageSource(Properties.Resources.Function) });
            FilterItems.Add(new FilterViewModel(this){Name = Resources.Title_Information, FilterImage = ResourceUtilities.ConvertToImageSource(Properties.Resources.Info) });
            FilterItems.Add(new FilterViewModel(this){Name = Resources.Title_IsInput, FilterImage = ResourceUtilities.ConvertToImageSource(Properties.Resources.IsInput) });
            FilterItems.Add(new FilterViewModel(this){Name = Resources.Title_IsOutput, FilterImage = ResourceUtilities.ConvertToImageSource(Properties.Resources.IsOutput) });
            FilterItems.Add(new FilterViewModel(this){Name = Resources.Title_MissingContent, FilterImage = ResourceUtilities.ConvertToImageSource(Properties.Resources.MissingNode) });
            FilterItems.Add(new FilterViewModel(this){Name = Resources.Title_Null, FilterImage = ResourceUtilities.ConvertToImageSource(Properties.Resources.Null) });
            FilterItems.Add(new FilterViewModel(this){Name = Resources.Title_Warning, FilterImage = ResourceUtilities.ConvertToImageSource(Properties.Resources.Alert) });
            FilterItems.Add(new FilterViewModel(this){Name = Resources.Title_PreviewOff, FilterImage = ResourceUtilities.ConvertToImageSource(Properties.Resources.Hidden) });

            filterDictionary = new Dictionary<string, FilterViewModel>(FilterItems.ToDictionary(fi => fi.Name));
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

            DisposeNodes();

            nodeDictionary.Clear();
            Nodes.Clear();

            foreach (var node in CurrentWorkspace.Nodes)
            {
                var graphNode = new GridNodeViewModel(node);
                graphNode.BubbleUpdate += (sender, args) => { RefreshNodesView(); };
                nodeDictionary[node.GUID] = graphNode;
                Nodes.Add(graphNode);
            }

            NodesCollection = new CollectionViewSource();
            NodesCollection.Source = Nodes;
            NodesCollection.Filter += NodesCollectionViewSource_Filter;

            RefreshNodesView();
        }

        /// <summary>
        /// A sequence of methods to update the DataGrid view
        /// </summary>
        private void RefreshNodesView()
        {
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

        /// <summary>
        /// Zoom around the currently selected Node
        /// </summary>
        /// <param name="obj"></param>
        internal void NodeSelect(object obj)
        {
            var nodeViewModel = obj as GridNodeViewModel;
            if (nodeViewModel == null) return;

            // Select
            var command = new DynamoModel.SelectModelCommand(nodeViewModel.NodeModel.GUID, ModifierKeys.None);  
            commandExecutive.ExecuteCommand(command, uniqueId, "GraphNodeManager");

            // Focus on selected
            viewModelCommandExecutive.FocusNodeCommand(nodeViewModel.NodeModel.GUID.ToString());
        }

        /// <summary>
        /// Switches off all filters
        /// </summary>
        /// <param name="obj"></param>
        internal void ClearAllFilters(object obj)
        {
            if (!FilterItems.Any()) return;

            foreach (FilterViewModel fvm in FilterItems)
            {
                fvm.IsFilterOn = false;
            }
            // Refresh the view 
            NodesCollectionFilter_Changed();
        }

        /// <summary>
        /// Export the current graph to CSV or JSON
        /// </summary>
        /// <param name="parameter"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal void ExportGraph(object parameter)
        {
            if (parameter == null) return;
            var type = parameter.ToString();
            var promptName =  System.IO.Path.GetFileNameWithoutExtension(currentWorkspace.FileName);

            var filteredNodes = FilteredNodesArray();

            switch (type)
            {
                case "CSV":
                    Utilities.Utilities.ExportToCSV(filteredNodes, promptName);
                    break;
                case "JSON":
                    Utilities.Utilities.ExportToJson(filteredNodes, promptName);
                    break;
            }
        }

        /// <summary>
        /// Helper method to return an Array of the currently active Nodes
        /// </summary>
        /// <returns></returns>
        private GridNodeViewModel [] FilteredNodesArray()
        {
            return GraphNodeManagerView.NodesInfoDataGrid.ItemsSource.Cast<GridNodeViewModel>().ToArray();
        }

        /// <summary>
        /// On changing a condition that affects the filter
        /// </summary>
        internal void NodesCollectionFilter_Changed()
        {
            // Refresh the view to apply filters.
            RaisePropertyChanged(nameof(IsAnyFilterOn));
            CollectionViewSource.GetDefaultView(GraphNodeManagerView.NodesInfoDataGrid.ItemsSource).Refresh();
        }

        /// <summary>
        /// Applies filter based on the Search Bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NodesCollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (!(e.Item is GridNodeViewModel nvm)) return;
            if (!filterDictionary.Any()) return;

            try
            {
                // Boolean Toggle Filters
                if (!nvm.IsEmptyList && filterDictionary[Resources.Title_EmptyList].IsFilterOn)
                {
                    e.Accepted = false;
                    return;
                }
                if (!nvm.IssuesHasError && filterDictionary[Resources.Title_Error].IsFilterOn)
                {
                    e.Accepted = false;
                    return;
                }
                if (!nvm.StatusIsFrozen && filterDictionary[Resources.Title_Frozen].IsFilterOn)
                {
                    e.Accepted = false;
                    return;
                }
                if (!nvm.IsDummyNode && filterDictionary[Resources.Title_MissingContent].IsFilterOn)
                {
                    e.Accepted = false;
                    return;
                }
                if (!nvm.StateIsFunction && filterDictionary[Resources.Title_Function].IsFilterOn)
                {
                    e.Accepted = false;
                    return;
                }
                if (!nvm.IsInfo && filterDictionary[Resources.Title_Information].IsFilterOn)
                {
                    e.Accepted = false;
                    return;
                }
                if (!nvm.StateIsInput && filterDictionary[Resources.Title_IsInput].IsFilterOn)
                {
                    e.Accepted = false;
                    return;
                }
                if (!nvm.StateIsOutput && filterDictionary[Resources.Title_IsOutput].IsFilterOn)
                {
                    e.Accepted = false;
                    return;
                }
                if (!nvm.IsNull && filterDictionary[Resources.Title_Null].IsFilterOn)
                {
                    e.Accepted = false;
                    return;
                }
                if (!nvm.IssuesHasWarning && filterDictionary[Resources.Title_Warning].IsFilterOn)
                {
                    e.Accepted = false;
                    return;
                }
                if (!nvm.StatusIsHidden && filterDictionary[Resources.Title_PreviewOff].IsFilterOn)
                {
                    e.Accepted = false;
                    return;
                }
            }
            catch(Exception err)
            {
                logMessage(Logging.LogMessage.Error(err));
            }

            // Textual SearchBox Filter
            if (string.IsNullOrEmpty(SearchText)) return;
            if (nvm.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                e.Accepted = true;
            else
            {
                e.Accepted = false;
                return;
            }

            e.Accepted = true;
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

            RaisePropertyChanged(nameof(Nodes));
            RaisePropertyChanged(nameof(NodesCollection));

            try
            {
                NodesCollection.Dispatcher.Invoke(() =>
                {
                    NodesCollection.SortDescriptions.Clear();

                    if (NodesCollection.View != null)
                        NodesCollection.View.Refresh();
                });
            }
            catch (InvalidOperationException)
            {
                return;
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// When a new node is added, update the collection
        /// </summary>
        /// <param name="node"></param>
        private void CurrentWorkspaceModel_NodeAdded(NodeModel node)
        {
            var infoNode = new GridNodeViewModel(node);
            infoNode.BubbleUpdate += (sender, args) => { RefreshNodesView(); };
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
            infoNode.BubbleUpdate -= (sender, args) => { RefreshNodesView(); };
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

            DisposeNodes();

            viewLoadedParams.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;
            viewLoadedParams.CurrentWorkspaceCleared -= OnCurrentWorkspaceCleared;
            NodesCollection.Filter -= NodesCollectionViewSource_Filter;
            GraphNodeManagerView = null;
        }

        /// <summary>
        /// Dispose of each NodeViewModel individually
        /// </summary>
        private void DisposeNodes()
        {
            foreach(var nvm in Nodes)
            {
                nvm.BubbleUpdate -= (sender, args) => { RefreshNodesView(); };
                nvm.Dispose();
            }
        }
        #endregion
    }
}
