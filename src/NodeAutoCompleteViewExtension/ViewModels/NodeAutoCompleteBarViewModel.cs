using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dynamo.Configuration;
using Dynamo.Engine;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Properties;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Dynamo.Wpf.ViewModels;
using Greg;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Mirror;
using ProtoCore.Utils;
using RestSharp;
using Dynamo.Wpf.Utilities;
using Dynamo.ViewModels;
using System.Reflection;
using Dynamo.Graph.Workspaces;
using Dynamo.Graph;
using System.Windows.Media;
using Dynamo.Selection;

namespace Dynamo.NodeAutoComplete.ViewModels
{
    /// <summary>
    /// Search View Model for Node AutoComplete Search Bar
    /// </summary>
    public class NodeAutoCompleteBarViewModel : SearchViewModel
    {
        internal PortViewModel PortViewModel { get; set; }
        private string autocompleteMLMessage;
        private string autocompleteMLTitle;
        private bool displayAutocompleteMLStaticPage;
        private bool displayLowConfidence;
        private const string nodeAutocompleteMLEndpoint = "MLNodeAutocomplete";
        private const string nodeClusterAutocompleteMLEndpoint = "MLNodeClusterAutocomplete";
        private const double minClusterConfidenceScore = 0.1;
        private static Assembly dynamoCoreWpfAssembly;

        private bool _isSingleAutocomplete;
        public bool IsInput => PortViewModel.PortType == PortType.Input;
        public bool SwitchIsEnabled => ResultsLoaded && !IsInput;
        public bool IsSingleAutocomplete
        {
            get => _isSingleAutocomplete || IsInput;
            set
            {
                if (PortViewModel.PortType == PortType.Output && _isSingleAutocomplete != value)
                {
                    _isSingleAutocomplete = value;
                    RaisePropertyChanged(nameof(IsSingleAutocomplete));

                    PopulateAutoComplete();
                }
            }
        }

        // Lucene search utility to perform indexing operations just for NodeAutocomplete.
        internal LuceneSearchUtility LuceneUtility
        {
            get
            {
                return LuceneSearch.LuceneUtilityNodeAutocomplete;
            }
        }

        /// <summary>
        /// The Node AutoComplete ML service version, this could be empty if user has not used ML way
        /// </summary>
        internal string ServiceVersion { get; set; }

        /// <summary>
        /// Cache of default node suggestions, use it in case where
        /// a. our algorithm does not return sufficient results
        /// b. the results returned by our algorithm will not be useful for user
        /// </summary>
        internal IEnumerable<NodeSearchElementViewModel> DefaultResults { get; set; }

        /// <summary>
        /// For checking if the ML method is selected
        /// </summary>
        public bool IsDisplayingMLRecommendation
        {
            get
            {
                return dynamoViewModel.PreferenceSettings.DefaultNodeAutocompleteSuggestion == Models.NodeAutocompleteSuggestion.MLRecommendation;
            }
        }

        /// <summary>
        /// If MLAutocompleteTOU is approved
        /// </summary>
        public bool IsMLAutocompleteTOUApproved
        {
            get
            {
                return dynamoViewModel.PreferenceSettings.IsMLAutocompleteTOUApproved;
            }
        }

        /// <summary>
        /// If true, autocomplete method options are hidden from UI 
        /// </summary>
        public bool HideAutocompleteMethodOptions
        {
            get
            {
                return dynamoViewModel.PreferenceSettings.HideAutocompleteMethodOptions;
            }
        }


        private IEnumerable<DNADropdownViewModel> dropdownResults;
        /// <summary>
        /// Cluster autocomplete search results.
        /// </summary>
        public IEnumerable<DNADropdownViewModel> DropdownResults
        {
            get
            {
                return dropdownResults;
            }
            set
            {
                dropdownResults = value;
                RaisePropertyChanged(nameof(DropdownResults));
                RaisePropertyChanged(nameof(NthofTotal));
                RaisePropertyChanged(nameof(ResultsLoaded));
                RaisePropertyChanged(nameof(SwitchIsEnabled));
                RaisePropertyChanged(nameof(ConfirmSource));
                RaisePropertyChanged(nameof(PreviousSource));
                RaisePropertyChanged(nameof(NextSource));
            }
        }

        /// <summary>
        /// Return the qualified results from the ML service above preferred confidence threshold
        /// </summary>
        internal IEnumerable<ClusterResultItem> QualifiedResults
        {
            get
            {
                if (FullResults == null)
                {
                    return null;
                }
                return FullResults.Results.Where(x => double.Parse(x.Probability) * 100 > minClusterConfidenceScore);
            }
        }

        public bool ResultsLoaded => DropdownResults != null;

        private bool isOpen;
        public bool IsOpen
        {
            get
            {
                return isOpen;
            }
            set
            {
                if (isOpen == value) return;
                isOpen = value;
                if (isOpen) SubscribeWindowEvents();
                else UnsubscribeWindowEvents();
            }
        }

        private int ClusterResultsCount => DropdownResults == null ? 0 : DropdownResults.Count();

        private int selectedIndex = 0;
        /// <summary>
        /// Selected index of the current cluster autocomplete option
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                /*don't try to add a node if the index is out of range or a selection is not made yet (-1)
                an index of -1 occurs when using the switch to change between modes.*/
                if (selectedIndex != value && value >= 0 && selectedIndex != -1)
                {
                    ReAddNode(value);
                }
                selectedIndex = value;
                RaisePropertyChanged(nameof(SelectedIndex));
                RaisePropertyChanged(nameof(NthofTotal));
                RaisePropertyChanged(nameof(PreviousSource));
                RaisePropertyChanged(nameof(NextSource));
            }
        }

        private void ReAddNode(int index)
        {
            if(FullResults == null)
            {
                return;
            }
            var results = QualifiedResults.ToList();
            if(index >=  0 && index  < results.Count)
            {
                AddCluster(results[index]);
            }
        }

        internal void ConsolidateTransientNodes()
        {
            var node = PortViewModel.NodeViewModel;
            var transientNodes = node.WorkspaceViewModel.Nodes.Where(x => x.IsTransient).ToList();
            foreach (var transientNode in transientNodes)
            {
                transientNode.IsTransient = false;
            }

            //set the last connector to be connected
            var transientConnectors = node.WorkspaceViewModel.Connectors.Where(c => c.IsConnecting).ToList();
            foreach (var connector in transientConnectors)
            {
                connector.IsConnecting = false;
            }

            NodeAutoCompleteUtilities.PostAutoLayoutNodes(node.WorkspaceViewModel.Model, node.NodeModel, transientNodes.Select(x => x.NodeModel), true, true, false, null);

            (node.WorkspaceViewModel.Model as HomeWorkspaceModel)?.MarkNodesAsModifiedAndRequestRun(transientNodes.Select(x => x.NodeModel));

            ToggleUndoRedoLocked(false);
        }

        internal void ToggleUndoRedoLocked(bool toggle = true)
        {
            var node = PortViewModel.NodeViewModel;
            //unlock undo/redo
            node.WorkspaceViewModel.Model.IsUndoRedoLocked = toggle;
            //allow for undo/redo again
            node.DynamoViewModel.RaiseCanExecuteUndoRedo();
        }

        /// <summary>
        /// Bitmap Source for left caret
        /// </summary>
        public string PreviousSource
        {
            get
            {
                return selectedIndex == 0 ? "/DynamoCoreWpf;component/UI/Images/caret-left-disabled.png" : "/DynamoCoreWpf;component/UI/Images/caret-left-default.png";
            }
        }
        /// <summary>
        /// Bitmap Source for right caret
        /// </summary>
        public string NextSource
        {
            get
            {
                return selectedIndex >= ClusterResultsCount - 1 ? "/DynamoCoreWpf;component/UI/Images/caret-right-disabled.png" : "/DynamoCoreWpf;component/UI/Images/caret-right-default.png";
            }
        }
        /// <summary>
        /// Bitmap Source for confirmation checkmark
        /// </summary>
        public string ConfirmSource
        {
            get
            {
                return ResultsLoaded ? "/DynamoCoreWpf;component/UI/Images/check.png" : "/DynamoCoreWpf;component/UI/Images/check-disabled.png";
            }
        }
        /// <summary>
        /// Language agnostic way of showing current result ordinal
        /// </summary>
        public string NthofTotal
        {
            get
            {
                return $"{selectedIndex + 1} / {ClusterResultsCount}";
            }
        }

        /// <summary>
        /// The No Recommendations or Low Confidence Title
        /// </summary>
        public string AutocompleteMLTitle
        {
            get { return autocompleteMLTitle; }
            set
            {
                autocompleteMLTitle = value;
                RaisePropertyChanged(nameof(AutocompleteMLTitle));
            }
        }

        /// <summary>
        /// The No Recommendations or Low Confidence message
        /// </summary>
        public string AutocompleteMLMessage
        {
            get { return autocompleteMLMessage; }
            set
            {
                autocompleteMLMessage = value;
                RaisePropertyChanged(nameof(AutocompleteMLMessage));
            }
        }

        /// <summary>
        /// Indicates the No recommendations / Low confidence message should be displayed (image and texts)
        /// </summary>
        public bool DisplayAutocompleteMLStaticPage
        {
            get { return displayAutocompleteMLStaticPage; }
            set
            {
                displayAutocompleteMLStaticPage = value;
                RaisePropertyChanged(nameof(DisplayAutocompleteMLStaticPage));
            }
        }

        /// <summary>
        /// Indicates if display the Low confidence option and Tooltip
        /// </summary>
        public bool DisplayLowConfidence
        {
            get { return displayLowConfidence; }
            set
            {
                displayLowConfidence = value;
                RaisePropertyChanged(nameof(DisplayLowConfidence));
            }
        }

        internal event Action<NodeModel> ParentNodeRemoved;

        internal MLNodeClusterAutoCompletionResponse FullResults { private set; get; }
        internal List<SingleResultItem> FullSingleResults { set; get; }
        private Guid LastRequestGuid;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dynamoViewModel">Dynamo ViewModel</param>
        internal NodeAutoCompleteBarViewModel(DynamoViewModel dynamoViewModel) : base(dynamoViewModel)
        {
            // Off load some time consuming operation here
            DefaultResults = dynamoViewModel.DefaultAutocompleteCandidates.Values;
            ServiceVersion = string.Empty;
        }

        /// <summary>
        /// Reset Node AutoComplete search view state
        /// </summary>
        internal void ResetAutoCompleteSearchViewState()
        {
            DisplayAutocompleteMLStaticPage = false;
            DisplayLowConfidence = dynamoViewModel.PreferenceSettings.HideNodesBelowSpecificConfidenceLevel && dynamoViewModel.PreferenceSettings.DefaultNodeAutocompleteSuggestion == NodeAutocompleteSuggestion.MLRecommendation;
            AutocompleteMLMessage = string.Empty;
            AutocompleteMLTitle = string.Empty;
            FilteredResults = new List<NodeSearchElementViewModel>();
            FilteredHighConfidenceResults = new List<NodeSearchElementViewModel>();
            FilteredLowConfidenceResults = new List<NodeSearchElementViewModel>();
        }

        internal MLNodeAutoCompletionRequest GenerateRequestForMLAutocomplete()
        {
            // Initialize request for the the ML API
            MLNodeAutoCompletionRequest request = new MLNodeAutoCompletionRequest(AssemblyHelper.GetDynamoVersion().ToString(), dynamoViewModel.PreferenceSettings.MLRecommendationNumberOfResults);

            var nodeInfo = PortViewModel.PortModel.Owner;
            var portInfo = PortViewModel.PortModel;

            // Set node info
            request.Node.Id = nodeInfo.GUID.ToString();
            request.Node.Lacing = nodeInfo.ArgumentLacing.ToString();

            if (nodeInfo is DSFunctionBase functionNode)
            {
                request.Node.Type.Id = functionNode.CreationName;
            }
            else if (nodeInfo is NodeModel nodeModel)
            {
                var typeID = new NodeModelTypeId(nodeModel.GetType().FullName, nodeModel.GetType().Assembly.GetName().Name);
                request.Node.Type.Id = typeID.ToString();
            }

            // Set port info
            // If the node is a Variable-input nodemodel or zero-touch node, then parse the port name to remove the digits at the end.
            request.Port.Name = (nodeInfo is VariableInputNode || nodeInfo is DSVarArgFunction) ? ParseVariableInputPortName(portInfo.Name) : portInfo.Name;
            request.Port.Index = portInfo.Index;
            request.Port.Direction = portInfo.PortType == PortType.Input ? PortType.Input.ToString().ToLower() : PortType.Output.ToString().ToLower();
            request.Port.KeepListStructure = portInfo.KeepListStructure.ToString();
            request.Port.ListAtLevel = portInfo.Level;

            // Set host info
            var hostName = string.IsNullOrEmpty(DynamoModel.HostAnalyticsInfo.HostName) ? dynamoViewModel.Model.HostName : DynamoModel.HostAnalyticsInfo.HostName;
            var hostNameEnum = GetHostNameEnum(hostName);

            if (hostNameEnum != HostNames.None)
            {
                request.Host = new HostItem(hostNameEnum.ToString(), dynamoViewModel.Model.HostVersion);
            }

            // Set packages info
            var packageManager = dynamoViewModel.Model.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();

            if (packageManager != null)
            {
                foreach (var pkg in packageManager.PackageLoader.LocalPackages)
                {
                    request.Packages = request.Packages.Append(new PackageItem(pkg.Name, pkg.VersionName));
                }
            }

            // Set context info which will contain all reachable nodes from the current node.
            var upstreamNodes = nodeInfo.AllUpstreamNodes(new List<NodeModel>());
            var downstreamNodes = nodeInfo.AllDownstreamNodes(new List<NodeModel>());

            var upstreamAndDownstreamNodes = new List<NodeModel>();
            upstreamAndDownstreamNodes.AddRange(upstreamNodes);
            upstreamAndDownstreamNodes.AddRange(downstreamNodes);

            foreach (NodeModel nodeModel in upstreamAndDownstreamNodes)
            {
                var nodeRequest = new NodeItem(nodeModel.GUID.ToString());

                if (nodeModel is DSFunctionBase DSfunctionNode)
                {
                    nodeRequest.Type.Id = DSfunctionNode.CreationName;
                }
                else if (nodeModel is NodeModel node)
                {
                    var typeID = new NodeModelTypeId(node.GetType().FullName, nodeModel.GetType().Assembly.GetName().Name);
                    nodeRequest.Type.Id = typeID.ToString();
                }
                request.Context.Nodes = request.Context.Nodes.Append(nodeRequest);
            }

            // Set info regarding all the connectors in the reachable component.
            var connectors = dynamoViewModel.CurrentSpaceViewModel.Model.Connectors;

            foreach (ConnectorModel connector in connectors)
            {
                var startNode = connector.Start.Owner;
                var endNode = connector.End.Owner;

                if (startNode.Equals(nodeInfo) || endNode.Equals(nodeInfo) || upstreamAndDownstreamNodes.Contains(startNode) || upstreamAndDownstreamNodes.Contains(endNode))
                {
                    var startPortName = (startNode is VariableInputNode || startNode is DSVarArgFunction) ? ParseVariableInputPortName(connector.Start.Name): connector.Start.Name;
                    var endPortName = (endNode is VariableInputNode || endNode is DSVarArgFunction) ? ParseVariableInputPortName(connector.End.Name) : connector.End.Name;

                    var connectorRequest = new ConnectionItem
                    {
                        StartNode = new ConnectorNodeItem(startNode.GUID.ToString(), startPortName),
                        EndNode = new ConnectorNodeItem(endNode.GUID.ToString(), endPortName)
                    };

                    request.Context.Connections = request.Context.Connections.Append(connectorRequest);
                }
            }

            return request;
        }

        private IEnumerable<SingleResultItem> GetNodeAutocompleMLResults()
        {
            MLNodeAutoCompletionResponse MLresults = null;

            // Get results from the ML API.
            try
            {
                MLresults = GetGenericAutocompleteResult<MLNodeAutoCompletionResponse>(nodeAutocompleteMLEndpoint);
            }
            catch (Exception ex)
            {
                dynamoViewModel.Model.Logger.Log("Unable to fetch ML Node autocomplete results: " + ex.Message);
                DisplayAutocompleteMLStaticPage = true;
                AutocompleteMLTitle = Resources.LoginNeededTitle;
                AutocompleteMLMessage = Resources.LoginNeededMessage;
                Analytics.TrackEvent(Actions.View, Categories.NodeAutoCompleteOperations, "UnabletoFetch");
                return new List<SingleResultItem>();
            }

            // no results
            if (MLresults == null || MLresults.Results.Count() == 0)
            {
                DisplayAutocompleteMLStaticPage = true;
                AutocompleteMLTitle = Resources.AutocompleteNoRecommendationsTitle;
                AutocompleteMLMessage = Resources.AutocompleteNoRecommendationsMessage;
                Analytics.TrackEvent(Actions.View, Categories.NodeAutoCompleteOperations, "NoRecommendation");
                return new List<SingleResultItem>();
            }
            ServiceVersion = MLresults.Version;
            var results = new List<SingleResultItem>();

            var zeroTouchSearchElements = Model.Entries.OfType<ZeroTouchSearchElement>().Where(x => x.IsVisibleInSearch);
            var nodeModelSearchElements = Model.Entries.OfType<NodeModelSearchElement>().Where(x => x.IsVisibleInSearch);

            // ML Results are categorized based on the threshold confidence score before displaying. 
            foreach (var result in MLresults.Results)
            {
                var portName = result.Port != null ? result.Port.Name : string.Empty;
                var portIndex = result.Port != null ? result.Port.Index : 0;

                // DS Function node
                if (result.Node.Type.NodeType.Equals(Function.FunctionNode))
                {
                    NodeSearchElement nodeSearchElement = null;
                    var element = zeroTouchSearchElements.FirstOrDefault(n => n.Descriptor.MangledName.Equals(result.Node.Type.Id));

                    if (element != null)
                    {
                        nodeSearchElement = (NodeSearchElement)element.Clone();

                        // Set PortToConnect for each element based on port-index and port-name
                        nodeSearchElement.AutoCompletionNodeElementInfo = new AutoCompletionNodeElementInfo
                        {   
                            PortToConnect = portIndex
                        };

                        foreach (var inputParameter in element.Descriptor.Parameters.Select((value, index) => (value, index)))
                        {
                            if (inputParameter.value.Name.Equals(portName))
                            {
                                nodeSearchElement.AutoCompletionNodeElementInfo.PortToConnect = element.Descriptor.Type == FunctionType.InstanceMethod ? inputParameter.index + 1 : inputParameter.index;
                                break;
                            }
                        }

                        var viewModelElement = new SingleResultItem(nodeSearchElement, result.Score);

                        results.Add(viewModelElement);
                    }
                }
                // Matching known node types of node-model nodes.
                else if (Enum.IsDefined(typeof(NodeModelNodeTypes), result.Node.Type.NodeType))
                {
                    // Retreive assembly name and full name from type id.
                    var typeInfo = GetInfoFromTypeId(result.Node.Type.Id);
                    string fullName = typeInfo.FullName;
                    string assemblyName = typeInfo.AssemblyName;
                    NodeSearchElement nodeSearchElement = null;

                    var nodesFromAssembly = nodeModelSearchElements.Where(n => Path.GetFileNameWithoutExtension(n.Assembly).Equals(assemblyName));
                    var element = nodesFromAssembly.FirstOrDefault(n => n.CreationName.Equals(fullName));

                    if (element != null)
                    {
                        nodeSearchElement = (NodeSearchElement)element.Clone();

                        nodeSearchElement.AutoCompletionNodeElementInfo = new AutoCompletionNodeElementInfo
                        {
                            PortToConnect = portIndex
                        };

                        var viewModelElement = new SingleResultItem(nodeSearchElement, result.Score);
                        results.Add(viewModelElement);
                    }
                }
            }

            return results;
        }
        private T GetGenericAutocompleteResult<T>(string endpoint)
        {   
            var requestDTO = GenerateRequestForMLAutocomplete();
            var jsonRequest = JsonConvert.SerializeObject(requestDTO);

#if DEBUG
            dynamoViewModel?.Model?.Logger?.Log(LogMessage.Info($"DNA Request: \n {jsonRequest}"));
#endif

            T results = default;
            try
            {
                var authProvider = dynamoViewModel?.Model?.AuthenticationManager?.AuthProvider;
                if (!dynamoViewModel.IsIDSDKInitialized())
                {
                    throw new Exception("IDSDK missing or failed initialization.");
                }

                if (authProvider is IOAuth2AuthProvider oauth2AuthProvider && authProvider is IOAuth2AccessTokenProvider tokenprovider)
                {
                    try
                    {
                        if (dynamoCoreWpfAssembly is null)
                        {
                            dynamoCoreWpfAssembly = AppDomain.CurrentDomain
                                .GetAssemblies()
                                .FirstOrDefault(a => a.GetName().Name.Equals("DynamoCoreWPF", StringComparison.OrdinalIgnoreCase));
                        }


                        var uri = DynamoUtilities.PathHelper.GetServiceBackendAddress(dynamoCoreWpfAssembly, endpoint);
                        var client = new RestClient(uri);
                        var request = new RestRequest(string.Empty, Method.Post);
                        var tkn = tokenprovider?.GetAccessToken();
                        if (string.IsNullOrEmpty(tkn))
                        {
                            throw new Exception("Authentication required.");
                        }
                        request.AddHeader("Authorization", $"Bearer {tkn}");
                        request = request.AddJsonBody(jsonRequest);
                        request.RequestFormat = DataFormat.Json;
                        RestResponse response = client.Execute(request);

                        results = JsonConvert.DeserializeObject<T>(response.Content);
                    }
                    catch (Exception ex)
                    {
                        dynamoViewModel.Model.Logger.Log(ex.Message);
                        throw new Exception("Authentication failed.");
                    }
                }
            }
            catch (Exception ex)
            {
                dynamoViewModel.Model.Logger.Log(ex.Message);
                throw new Exception("Authentication failed.");
            }

            return results;
        }

        /// <summary>
        /// Show the low confidence ML results.
        /// </summary>
        internal void ShowLowConfidenceResults()
        {
            DisplayLowConfidence = false;
            DisplayAutocompleteMLStaticPage = false;
            IEnumerable<NodeSearchElementViewModel> allResults = FilteredHighConfidenceResults.Concat(FilteredLowConfidenceResults);
            FilteredResults = allResults;
        }

        // Full name and assembly name 
        internal NodeModelTypeId GetInfoFromTypeId(string typeId)
        {
            if (typeId.Contains(','))
            {
                var type = typeId.Split(',');
                return new NodeModelTypeId(type[0].Trim(), type[1].Trim());
            }

            return new NodeModelTypeId(typeId);
        }

        // Remove the digits at the end of the portname for variable input node
        private string ParseVariableInputPortName(string portName)
        {
            string pattern = @"\d+$";
            Regex rgx = new Regex(pattern);
            return rgx.Replace(portName, string.Empty);
        }

        // Get the host name from the enum list.
        internal HostNames GetHostNameEnum(string HostName)
        {
            switch (HostName)
            {
                case string name when name.IndexOf("Revit", StringComparison.OrdinalIgnoreCase) >= 0:
                    return HostNames.Revit;
                case string name when name.IndexOf("Civil", StringComparison.OrdinalIgnoreCase) >= 0:
                    return HostNames.Civil3d;
                case string name when name.IndexOf("Alias", StringComparison.OrdinalIgnoreCase) >= 0:
                    return HostNames.Alias;
                case string name when name.IndexOf("FormIt", StringComparison.OrdinalIgnoreCase) >= 0:
                    return HostNames.FormIt;
                case string name when name.IndexOf("Steel", StringComparison.OrdinalIgnoreCase) >= 0:
                    return HostNames.AdvanceSteel;
                case string name when name.IndexOf("RSA", StringComparison.OrdinalIgnoreCase) >= 0:
                    return HostNames.RSA;
                default:
                    return HostNames.None;
            }
        }

        /// <summary>
        /// Key function to populate node autocomplete results to display
        /// </summary>
        internal IEnumerable<SingleResultItem> GetSingleAutocompleteResults()
        {
            if (PortViewModel == null) return null;

            if (IsDisplayingMLRecommendation)
            {
                //Tracking Analytics when raising Node Autocomplete with the Recommended Nodes option selected (Machine Learning)
                Analytics.TrackEvent(
                    Actions.Show,
                    Categories.NodeAutoCompleteOperations,
                    nameof(NodeAutocompleteSuggestion.MLRecommendation));
                return GetNodeAutocompleMLResults();
            }
            else
            {
                //Tracking Analytics when raising Node Autocomplete with the Object Types option selected.
                Analytics.TrackEvent(
                    Actions.Show,
                    Categories.NodeAutoCompleteOperations,
                    nameof(NodeAutocompleteSuggestion.ObjectType));
                // Only call GetMatchingSearchElements() for object type match comparison
                var objectTypeMatchingElements = GetMatchingSearchElements().ToList();
                // If node match searchElements found, use default suggestions. 
                // These default suggestions will be populated based on the port type.
                if (!objectTypeMatchingElements.Any())
                {
                    return DefaultAutoCompleteCandidates().Select(x => new SingleResultItem(x.Model, 1.0));
                }
                else
                {
                    return objectTypeMatchingElements.Select(x => new SingleResultItem(x, 1.0));
                }
            }
        }

        // Delete all transient nodes in the workspace
        internal void DeleteTransientNodes()
        {
            var node = PortViewModel.NodeViewModel;
            var wsViewModel = node.WorkspaceViewModel;

            var transientNodes = wsViewModel.Nodes.Where(x => x.IsTransient).ToList();
            if (transientNodes.Any())
            {
                dynamoViewModel.Model.ExecuteCommand(new DynamoModel.DeleteModelCommand(transientNodes.Select(x => x.Id), true));
                //remove the deletion of the elements from the undo stack
                wsViewModel.Model.UndoRecorder.PopFromUndoGroup();
                //remove the layout of the elements from the undo stack
                wsViewModel.Model.UndoRecorder.PopFromUndoGroup();
            }
        }

        // Add Cluster from server result into the workspace
        internal void AddCluster(ClusterResultItem clusterResultItem)
        {
            if (clusterResultItem == null || clusterResultItem.Topology == null)
                return;

            List<ModelBase> createdClusterItems = new List<ModelBase>();

            var workspaceViewModel = PortViewModel.NodeViewModel.WorkspaceViewModel;
            var workspaceModel = workspaceViewModel.Model;
            var dynamoModel = PortViewModel.NodeViewModel.DynamoViewModel.Model;
            var entryNodeId = clusterResultItem.Topology.Nodes.ElementAtOrDefault(clusterResultItem.EntryNodeIndex)?.Id;

            // Lock undo/redo
            ToggleUndoRedoLocked(true);

            // Delete any existing transient nodes
            DeleteTransientNodes();

            // Map to store created nodes for connection lookup
            var createdNodes = new Dictionary<string, NodeModel>();

            // Create nodes from the cluster topology
            var offset = PortViewModel.NodeViewModel.X + PortViewModel.NodeViewModel.NodeModel.Width;

            List<List<NodeItem>> nodeStacks = NodeAutoCompleteUtilities.ComputeNodePlacementHeuristics(clusterResultItem.Topology.Connections.ToList(), clusterResultItem.Topology.Nodes.ToList());

            foreach (var nodeStack in nodeStacks)
            {
                offset += PortViewModel.NodeViewModel.NodeModel.Width;
                foreach (var nodeItem in nodeStack)
                {
                    var typeInfo = new NodeModelTypeId(nodeItem.Type.Id);
                    var newNode = dynamoModel.CreateNodeFromNameOrType(Guid.NewGuid(), typeInfo.FullName, true);
                    if (newNode != null)
                    {
                        newNode.X = offset; // Adjust X position
                        newNode.Y = PortViewModel.NodeViewModel.NodeModel.Y; // Adjust Y position
                        workspaceModel.AddAndRegisterNode(newNode);
                        createdNodes[nodeItem.Id] = newNode;
                        createdClusterItems.Add(newNode);

                        var newNodeViewModel = workspaceViewModel.Nodes.Last();
                        newNodeViewModel.IsHidden = true; // Hide the node initially
                    }
                }
            }

            // Connect the cluster to the original node and port
            if (entryNodeId != null && createdNodes.TryGetValue(entryNodeId, out var entryNode))
            {

                ConnectorModel entryConnector = null;
                if (PortViewModel.PortType == PortType.Output)
                {
                    var portIndex = clusterResultItem.EntryNodeInPort;
                    if (entryNode.InPorts.Count > portIndex &&!entryNode.InPorts[portIndex].Connectors.Any())
                    {
                        entryConnector = ConnectorModel.Make(PortViewModel.NodeViewModel.NodeModel, entryNode, PortViewModel.PortModel.Index, portIndex);
                    }
                }
                else
                {
                    var portIndex = clusterResultItem.EntryNodeOutPort;
                    if (entryNode.OutPorts.Count > portIndex && !entryNode.OutPorts[portIndex].Connectors.Any())
                    {
                        entryConnector = ConnectorModel.Make(entryNode, PortViewModel.NodeViewModel.NodeModel, portIndex, PortViewModel.PortModel.Index);
                    }
                }
                if (entryConnector != null)
                {
                    entryConnector.IsHidden = true;
                    var entryConnectorViewModel = workspaceViewModel.Connectors.First(c => c.ConnectorModel.Equals(entryConnector));
                    entryConnectorViewModel.IsConnecting = true;
                    createdClusterItems.Add(entryConnector);
                }
            }

            // Create connections between nodes
            foreach (var connection in clusterResultItem.Topology.Connections)
            {
                if (createdNodes.TryGetValue(connection.StartNode.NodeId, out var sourceNode) &&
                    createdNodes.TryGetValue(connection.EndNode.NodeId, out var targetNode))
                {
                    var sourcePortIndex = connection.StartNode.PortIndex - 1;
                    var targetPortIndex = connection.EndNode.PortIndex - 1;

                    if (sourceNode.OutPorts.Count > sourcePortIndex && targetNode.InPorts.Count > targetPortIndex)
                    {
                        if (!targetNode.InPorts[targetPortIndex].Connectors.Any())
                        {
                            var newConnector = ConnectorModel.Make(sourceNode, targetNode, sourcePortIndex, targetPortIndex);

                            if (newConnector != null)
                            {
                                newConnector.IsHidden = true; // Hide the connector initially
                                createdClusterItems.Add(newConnector);
                            }
                        }
                    }
                }
            }

            //add the new items to the undo recorder (this ensures the elements are valid at this point in time before any other manipulation occurs)
            DynamoModel.RecordUndoModels(workspaceModel, createdClusterItems);

            // Perform auto-layout for the newly added nodes
            NodeAutoCompleteUtilities.PostAutoLayoutNodes(
                workspaceViewModel.DynamoViewModel.CurrentSpace,
                PortViewModel.NodeViewModel.NodeModel,
                createdNodes.Values,
                false,
                false,
                false,
                () =>
                {
                    // Finalize visibility of nodes and connectors
                    foreach (var node in createdNodes.Values)
                    {
                        var matchingNode = workspaceViewModel.Nodes.FirstOrDefault(n => n.NodeModel.GUID.Equals(node.GUID));
                        if (matchingNode != null)
                        {
                            matchingNode.IsHidden = false;
                        }
                        foreach (var connector in node.AllConnectors)
                        {
                            connector.IsHidden = !PreferenceSettings.Instance.ShowConnector;
                        }
                    }
                });
        }

        /// <summary>
        /// Key function to populate node autocomplete results to display
        /// </summary>
        internal void PopulateAutoComplete()
        {
            if (PortViewModel == null) return;

            ResetAutoCompleteSearchViewState();

            FullResults = null;
            if(DropdownResults != null)
            {
                DropdownResults = null;
            }

            //this should run on the UI thread, so thread safety is not a concern
            LastRequestGuid = Guid.NewGuid();
            var myRequest = LastRequestGuid;

            //start a background thread to make the http request
            Task.Run(() =>
            {
                List<SingleResultItem> fullSingleResults = null;
                MLNodeClusterAutoCompletionResponse fullResults = null;

                if (IsSingleAutocomplete || !IsDisplayingMLRecommendation)
                {
                    fullSingleResults = GetSingleAutocompleteResults().ToList();
                    fullResults = new MLNodeClusterAutoCompletionResponse
                    {
                        Version = "0.0",
                        NumberOfResults = fullSingleResults.Count,
                        Results = fullSingleResults.Select(x => new ClusterResultItem
                        {
                            Description = x.Description,
                            Title = x.Description,  
                            Probability = x.Score.ToString(),
                            EntryNodeIndex = 0,
                            EntryNodeInPort = PortViewModel.PortType == PortType.Output ? x.PortToConnect : -1,
                            EntryNodeOutPort = PortViewModel.PortType == PortType.Input ? x.PortToConnect : -1,
                            Topology = new TopologyItem
                            {
                                Nodes = new List<NodeItem> { new NodeItem {
                                    Id = new Guid().ToString(),
                                    Type = new NodeType { Id = x.CreationName } } },
                                Connections = new List<ConnectionItem>()
                            }
                        })
                    };
                }
                else
                {
                    fullResults = GetGenericAutocompleteResult<MLNodeClusterAutoCompletionResponse>(nodeClusterAutocompleteMLEndpoint);
                }

                dynamoViewModel.UIDispatcher.BeginInvoke(() =>
                {
                    if(LastRequestGuid != myRequest)
                    {
                        //a newer request came, we're no longer interested in the results of this one
                        //only latest request has the right to be committed to the UI and internal data structures
                        return;
                    }
                    if (!IsOpen)
                    {
                        // view disappeared while the background thread was waiting for the server response.
                        // Ignore the results are we're no longer interested.
                        return;
                    }

                    FullSingleResults = fullSingleResults ?? FullSingleResults;
                    FullResults = fullResults ?? FullResults;

                    IEnumerable<DNADropdownViewModel> comboboxResults;
                    if (IsSingleAutocomplete || !IsDisplayingMLRecommendation)
                    {
                        //getting bitmaps from resources necessarily has to be done in the UI thread
                        Dictionary<string, ImageSource> dict = [];
                        foreach (var singleResult in FullSingleResults)
                        {
                            if (dict.ContainsKey(singleResult.CreationName))
                            {
                                continue;
                            }
                            var iconRequest = new IconRequestEventArgs(singleResult.Assembly, singleResult.IconName + Configurations.SmallIconPostfix);
                            SearchViewModelRequestBitmapSource(iconRequest);
                            dict[singleResult.CreationName] = iconRequest.Icon;
                        }
                        comboboxResults = QualifiedResults.Select(x => new DNADropdownViewModel
                        {
                            Description = x.Description,
                            SmallIcon = dict[x.Topology.Nodes.First().Type.Id],
                        });
                    }
                    else
                    {
                        comboboxResults = QualifiedResults.Select(x => new DNADropdownViewModel
                        {
                            Description = x.Description
                            //default icon (cluster) is set in the xaml view
                        });
                    }
                    // this runs synchronously on the UI thread, so the UI can't disappear during execution
                    DropdownResults = comboboxResults;
                    SelectedIndex = 0;

                    var ClusterResultItem = QualifiedResults.First();
                    AddCluster(ClusterResultItem);
                    
                });
            });
            //Tracking Analytics when raising Node Autocomplete with the Recommended Nodes option selected (Machine Learning)
            Analytics.TrackEvent(
                Actions.Show,
                Categories.NodeAutoCompleteOperations,
                nameof(NodeAutocompleteSuggestion.MLRecommendation));
        }

        internal IEnumerable<NodeSearchElementViewModel> DefaultAutoCompleteCandidates()
        {
            if (PortViewModel.PortModel.PortType == PortType.Input)
            {
                switch (PortViewModel.PortModel.GetInputPortType())
                {
                    case "int":
                        return DefaultResults.Where(e => e.Name == "Number Slider" || e.Name == "Integer Slider").ToList();
                    case "double":
                        return DefaultResults.Where(e => e.Name == "Number Slider" || e.Name == "Integer Slider").ToList();
                    case "string":
                        return DefaultResults.Where(e => e.Name == "String").ToList();
                    case "bool":
                        return DefaultResults.Where(e => e.Name == "Boolean").ToList();
                    default:
                        return DefaultResults.Where(e => e.Name == "String" || e.Name == "Number Slider" || e.Name == "Integer Slider" || e.Name == "Number" || e.Name == "Boolean");
                }
            }
            else
            {
                return DefaultResults.Where(e => e.Name == "Watch" || e.Name == "Watch 3D" || e.Name == "Python Script").ToList();
            }
        }

        private void OnPreferencesChanged()
        {
            RaisePropertyChanged(nameof(IsDisplayingMLRecommendation));
            PopulateAutoComplete();
        }

        private void SubscribeWindowEvents()
        {
            dynamoViewModel.CurrentSpaceViewModel.Model.NodeRemoved += NodeViewModel_Removed;
            dynamoViewModel.PreferenceSettings.AutocompletePreferencesChanged += OnPreferencesChanged;
        }

        private void UnsubscribeWindowEvents()
        {
            dynamoViewModel.CurrentSpaceViewModel.Model.NodeRemoved -= NodeViewModel_Removed;
            dynamoViewModel.PreferenceSettings.AutocompletePreferencesChanged -= OnPreferencesChanged;
        }

        internal void NodeViewModel_Removed(NodeModel node)
        {
            ParentNodeRemoved?.Invoke(node);
        }

        /// <summary>
        /// Returns a collection of node search elements for nodes
        /// that output a type compatible with the port type if it's an input port.
        /// These search elements can belong to either zero touch, NodeModel or Builtin nodes.
        /// This method returns an empty collection if the input port type cannot be inferred or
        /// there are no matching nodes found for the type. Currently the match does not include
        /// rank information. For example Curve[] is matched as Curve. The results include types
        /// that would be valid using normal class inheritance rules.
        /// The resulting compatible search elements can be made to appear in the node autocomplete search dialog.
        /// </summary>
        /// <returns>collection of node search elements</returns>
        internal IEnumerable<NodeSearchElement> GetMatchingSearchElements()
        {
            var elements = new List<NodeSearchElement>();

            var portType = String.Empty;

            if (PortViewModel.PortModel.PortType == PortType.Input)
            {
                portType = PortViewModel.PortModel.GetInputPortType();
            }
            else if (PortViewModel.PortModel.PortType == PortType.Output)
            {
                portType = PortViewModel.PortModel.GetOutPortType();
                //if the custom node output name contains spaces, try using the first word.
                if (PortViewModel.PortModel.Owner is Graph.Nodes.CustomNodes.Function && portType.Any(char.IsWhiteSpace))
                {
                    portType = string.Concat(portType.TrimStart().TakeWhile(char.IsLetterOrDigit));
                }
            }

            //List of input types that are skipped temporarily, and will display list of default suggestions instead.
            var skippedInputTypes = new List<string>() { "var", "object", "string", "bool", "int", "double" };

            if (portType == null)
            {
                return elements;
            }

            var core = dynamoViewModel.Model.LibraryServices.LibraryManagementCore;

            //if inputPortType is an array, use just the typename
            ParseResult parseResult = null;
            try
            {
                parseResult = ParserUtils.ParseWithCore($"dummyName:{portType};", core);
            }
            catch (ProtoCore.BuildHaltException)
            { }

            var ast = parseResult?.CodeBlockNode.Children().FirstOrDefault() as IdentifierNode;
            //if parsing the type failed, revert to original string.
            portType = ast != null ? ast.datatype.Name : portType;
            //check if the input port return type is in the skipped input types list
            if (skippedInputTypes.Any(s => s == portType))
            {
                return elements;
            }

            //gather all ztsearchelements or nodemodel nodes that are visible in search and filter using inputPortType and zt return type name.
            var ztSearchElements = Model.Entries.OfType<ZeroTouchSearchElement>().Where(x => x.IsVisibleInSearch);
            var nodeModelSearchElements = Model.Entries.OfType<NodeModelSearchElement>().Where(x => x.IsVisibleInSearch);

            if (PortViewModel.PortModel.PortType == PortType.Input)
            {
                foreach (var ztSearchElement in ztSearchElements)
                {
                    //for now, remove rank from descriptors    
                    var returnTypeName = ztSearchElement.Descriptor.ReturnType.Name;

                    var descriptor = ztSearchElement.Descriptor;
                    if ((returnTypeName == portType) || DerivesFrom(portType, returnTypeName, core))
                    {
                        elements.Add(ztSearchElement);
                    }
                }

                // NodeModel nodes, match any output return type to inputport type name
                foreach (var element in nodeModelSearchElements)
                {
                    if (element.OutputParameters.Any(op => op == portType))
                    {
                        elements.Add(element);
                    }
                }
            }
            else if (PortViewModel.PortModel.PortType == PortType.Output)
            {
                foreach (var ztSearchElement in ztSearchElements)
                {
                    foreach (var inputParameter in ztSearchElement.Descriptor.Parameters.Select((value, index) => new { value, index }))
                    {
                        var ZTparamName = inputParameter.value.Type.Name ?? inputParameter.value.Type.ToString();
                        if (ZTparamName == portType || DerivesFrom(ZTparamName, portType, core))
                        {
                            ztSearchElement.AutoCompletionNodeElementInfo.PortToConnect = ztSearchElement.Descriptor.Type == FunctionType.InstanceMethod ? inputParameter.index + 1 : inputParameter.index;
                            elements.Add(ztSearchElement);
                            break;
                        }
                    }
                }

                // NodeModel nodes, match any output return type to inputport type name
                foreach (var element in nodeModelSearchElements)
                {
                    foreach (var inputParameter in element.InputParameters)
                    {
                        if (inputParameter.Item2 == portType)
                        {
                            elements.Add(element);
                        }
                    }
                }
            }

            var comparer = new NodeSearchElementComparer(portType, core);

            //first sort by type distance to input port type
            elements.Sort(comparer);
            //then sort by node library group (create, action, or query node)
            //this results in a list of elements with 3 major groups(create,action,query), each group is sub sorted into types and then sorted by name.
            return elements.OrderBy(x => x.Group).ThenBy(x => x.OutputParameters.FirstOrDefault().ToString()).ThenBy(x => x.Name);
        }

        /// <summary>
        /// Does typeb derive from typea
        /// </summary>
        /// <param name="typea"></param>
        /// <param name="typeb"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        private bool DerivesFrom(string typea, string typeb, ProtoCore.Core core)
        {
            try
            {
                //TODO mirrors can be cached until new types are imported...
                var mirror1 = new ClassMirror(typea, core);
                var mirror2 = new ClassMirror(typeb, core);

                //TODO as we do this check we can cache the type distance...
                if (mirror2.GetClassHierarchy().Any(x => x.ClassName == mirror1.ClassName))
                {
                    //this is a derived type
                    return true;
                }
                return false;
            }
            catch
            {
                Debug.WriteLine($"failed to create class mirror for either {typea} or {typeb} during node autocomplete operation ");
                return false;
            }
        }

        /// <summary>
        /// Compares NodeSearchElements based on their typeDistance from a given type 'typeNameToCompareTo'
        /// </summary>
        internal class NodeSearchElementComparer : IComparer<NodeSearchElement>
        {
            private string typeNameToCompareTo;
            private ProtoCore.Core core;

            internal NodeSearchElementComparer(string typeNameToCompareTo, ProtoCore.Core core)
            {
                this.typeNameToCompareTo = typeNameToCompareTo;
                this.core = core;
            }

            public int Compare(NodeSearchElement x, NodeSearchElement y)
            {
                return CompareNodeSearchElementByTypeDistance(x, y, typeNameToCompareTo, core);
            }


            /// <summary>
            /// Compares two nodeSearchElements - general rules of the sort as follows:
            /// If all return types of the two searchElements are the same, they are equal.
            /// If any return type of both searchElements is an exact match for our input type, they are equal.
            /// If a searchElement is null, it is larger than the other element.
            /// If a single searchElement's return type list contains an exact match it is smaller.
            /// If the minimum type distance between a searchElements return types and our inputType than this searchElement is smaller. (closer)
            /// If the minimim type distances are the same the searchElements are equal.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="typeNameToCompareTo"></param>
            /// <param name="core"></param>
            /// <returns></returns>
            private int CompareNodeSearchElementByTypeDistance(NodeSearchElement x, NodeSearchElement y, string typeNameToCompareTo, ProtoCore.Core core)
            {
                /*
                 * -1	x precedes y.
                 0 x and y are equal
                 1	x is after y.
                 */
                var none = new string[] { Resources.NoneString };
                var xTypeNames = x.OutputParameters;
                var yTypeNames = y.OutputParameters;

                //if either element is a ztsearchElement then just use the type name (this strips off the rank)
                if (x is ZeroTouchSearchElement xzt)
                {
                    xTypeNames = new string[] { xzt.Descriptor.ReturnType.Name };
                }
                // for non ZT nodes, we don't have concrete return types, so we need to parse the typename. 
                else
                {
                    //if inputPortType is an array, lets just use the class name to match 
                    xTypeNames = xTypeNames.Select(type => (ParserUtils.ParseWithCore($"dummyName:{type};", core).
                    CodeBlockNode.Children().ElementAt(0) as TypedIdentifierNode).datatype.Name);
                }
                if (y is ZeroTouchSearchElement yzt)
                {
                    yTypeNames = new string[] { yzt.Descriptor.ReturnType.Name };
                }
                // for non ZT nodes, we don't have concrete return types, so we need to parse the typename. 
                else
                {
                    yTypeNames = yTypeNames.Select(type => (ParserUtils.ParseWithCore($"dummyName:{type};", core).
                    CodeBlockNode.Children().ElementAt(0) as TypedIdentifierNode).datatype.Name);
                }

                if (xTypeNames.SequenceEqual(yTypeNames))
                {
                    return 0;
                }

                // x and y are equal because both typeLists contain an exact match
                if (xTypeNames.Any(xType => xType == typeNameToCompareTo) && (yTypeNames.Any(yType => yType == typeNameToCompareTo)))
                {
                    return 0;
                }

                // null is further away, so x is at the end of list.
                if (xTypeNames.SequenceEqual(none))
                {
                    return 1;
                }
                // null is further away, so y is at the end of the list.
                if (yTypeNames.SequenceEqual(none))
                {
                    return -1;
                }

                // x precedes y because it contains an exact match
                if (xTypeNames.Any(xType => xType == typeNameToCompareTo))
                {
                    return -1;
                }
                //  y precedes x because it contains an exact match
                if (yTypeNames.Any(yType => yType == typeNameToCompareTo))
                {
                    return 1;
                }

                var xminDistance = xTypeNames.Select(name => GetTypeDistance(typeNameToCompareTo, name, core)).Min();
                var yminDistance = yTypeNames.Select(name => GetTypeDistance(typeNameToCompareTo, name, core)).Min();

                //if distance of x to currentSelectedType is greater than y distance
                //then x is further away
                if (xminDistance > yminDistance)
                {
                    return 1;
                }
                if (xminDistance == yminDistance)
                {
                    return 0;
                }
                // distance2 < distance 1
                // x precedes y
                return -1;
            }

            /// <summary>
            /// Return the type distance between two type names. 
            /// </summary>
            /// <param name="typea"></param>
            /// <param name="typeb"></param>
            /// <param name="core"></param>
            /// <returns>Will return int.MaxValue if no match can be found.
            /// Otherwise will return the distance between two types in class hierarchy.
            /// Will throw an exception if either type name is undefined.
            ///</returns>
            private static int GetTypeDistance(string typea, string typeb, ProtoCore.Core core)
            {
                //TODO - cache? Turn into params?
                var mirror1 = new ClassMirror(typea, core);
                var mirror2 = new ClassMirror(typeb, core);

                if (mirror1.ClassNodeID == mirror2.ClassNodeID)
                {
                    return 0;
                }

                var heirarchy = mirror2.GetClassHierarchy();
                var dist = 0;
                while (dist < heirarchy.Count())
                {
                    if (heirarchy.ElementAt(dist).ClassName == mirror1.ClassName)
                    {
                        return dist + 1;
                    }
                    dist++;
                }
                //if we can't find a match then dist should indicate that.
                return int.MaxValue;
            }

        }
    }
}
