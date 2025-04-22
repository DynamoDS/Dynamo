using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
using J2N.Text;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Mirror;
using ProtoCore.Utils;
using RestSharp;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// Search View Model for Node AutoComplete Search Bar
    /// </summary>
    [Obsolete("This class will be removed in a future version of Dynamo")]
    public class NodeAutoCompleteSearchViewModel : SearchViewModel
    {
        internal PortViewModel PortViewModel { get; set; }
        private List<NodeSearchElementViewModel> searchElementsCache;
        private string autocompleteMLMessage;
        private string autocompleteMLTitle;
        private bool displayAutocompleteMLStaticPage;
        private bool displayLowConfidence;
        private const string nodeAutocompleteMLEndpoint = "MLNodeAutocomplete";
        private const string nodeClusterAutocompleteMLEndpoint = "MLNodeClusterAutocomplete";

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
        [Obsolete("This method will be removed in a future version of Dynamo")]
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
        [Obsolete("This method will be removed in a future version of Dynamo")]
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
        [Obsolete("This method will be removed in a future version of Dynamo")]
        public bool HideAutocompleteMethodOptions
        {
            get
            {
                return dynamoViewModel.PreferenceSettings.HideAutocompleteMethodOptions;
            }
        }

        /// <summary>
        /// The No Recommendations or Low Confidence Title
        /// </summary>
        [Obsolete("This method will be removed in a future version of Dynamo")]
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
        [Obsolete("This method will be removed in a future version of Dynamo")]
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
        [Obsolete("This method will be removed in a future version of Dynamo")]
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
        [Obsolete("This method will be removed in a future version of Dynamo")]
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dynamoViewModel">Dynamo ViewModel</param>
        internal NodeAutoCompleteSearchViewModel(DynamoViewModel dynamoViewModel) : base(dynamoViewModel)
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
            searchElementsCache = new List<NodeSearchElementViewModel>();
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

        internal void ShowNodeAutocompleMLResults()
        {
            MLNodeAutoCompletionResponse MLresults = null;

            var request = GenerateRequestForMLAutocomplete();

            string jsonRequest = JsonConvert.SerializeObject(request);

            // Get results from the ML API.
            try
            {
                MLresults = GetMLNodeAutocompleteResults(jsonRequest);
            }
            catch (Exception ex)
            {
                dynamoViewModel.Model.Logger.Log("Unable to fetch ML Node autocomplete results: " + ex.Message);
                DisplayAutocompleteMLStaticPage = true;
                AutocompleteMLTitle = Resources.LoginNeededTitle;
                AutocompleteMLMessage = Resources.LoginNeededMessage;
                Analytics.TrackEvent(Actions.View, Categories.NodeAutoCompleteOperations, "UnabletoFetch");
                return;
            }

            // no results
            if (MLresults == null || MLresults.Results.Count() == 0)
            {
                DisplayAutocompleteMLStaticPage = true;
                AutocompleteMLTitle = Resources.AutocompleteNoRecommendationsTitle;
                AutocompleteMLMessage = Resources.AutocompleteNoRecommendationsMessage;
                Analytics.TrackEvent(Actions.View, Categories.NodeAutoCompleteOperations, "NoRecommendation");
                return;
            }
            ServiceVersion = MLresults.Version;
            var results = new List<NodeSearchElementViewModel>();

            var zeroTouchSearchElements = Model.Entries.OfType<ZeroTouchSearchElement>().Where(x => x.IsVisibleInSearch);
            var nodeModelSearchElements = Model.Entries.OfType<NodeModelSearchElement>().Where(x => x.IsVisibleInSearch);

            // ML Results are categorized based on the threshold confidence score before displaying. 
            if (MLresults.Results.Count() > 0)
            {
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
                        }

                        // Set PortToConnect for each element based on port-index and port-name
                        if (nodeSearchElement != null)
                        {
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
                        }

                        var viewModelElement = GetViewModelForNodeSearchElement(nodeSearchElement);

                        if (viewModelElement != null)
                        {
                            viewModelElement.AutoCompletionNodeMachineLearningInfo = new AutoCompletionNodeMachineLearningInfo(true, true, Math.Round(result.Score * 100));
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
                        }

                        if (nodeSearchElement != null)
                        {
                            nodeSearchElement.AutoCompletionNodeElementInfo = new AutoCompletionNodeElementInfo
                            {
                                PortToConnect = portIndex
                            };
                        }

                        var viewModelElement = GetViewModelForNodeSearchElement(nodeSearchElement);

                        if (viewModelElement != null)
                        {
                            viewModelElement.AutoCompletionNodeMachineLearningInfo = new AutoCompletionNodeMachineLearningInfo(true, true, Math.Round(result.Score * 100));
                            results.Add(viewModelElement);
                        }
                    }
                }

                OrganizeConfidenceSection(results);
            }
        }

        /// <summary>
        /// Compare to low confidence threadhold defined by user can origanize the results into high and low confidence sections.
        /// </summary>
        /// <param name="results"></param>
        internal void OrganizeConfidenceSection(List<NodeSearchElementViewModel> results)
        {
            foreach (var result in results)
            {
                if (result.AutoCompletionNodeMachineLearningInfo.ConfidenceScore >= dynamoViewModel.PreferenceSettings.MLRecommendationConfidenceLevel)
                {
                    FilteredHighConfidenceResults = FilteredHighConfidenceResults.Append(result);
                }
                else
                {
                    FilteredLowConfidenceResults = FilteredLowConfidenceResults.Append(result);
                }
            }

            // Show low confidence section if there are some results under threshold and feature enabled
            DisplayLowConfidence = FilteredLowConfidenceResults.Any() && dynamoViewModel.PreferenceSettings.HideNodesBelowSpecificConfidenceLevel;

            if (!FilteredHighConfidenceResults.Any())
            {
                DisplayAutocompleteMLStaticPage = true;
                AutocompleteMLTitle = Resources.AutocompleteLowConfidenceTitle;
                AutocompleteMLMessage = Resources.AutocompleteLowConfidenceMessage;
                return;
            }

            // By default, show only the results which are above the threshold
            FilteredResults = dynamoViewModel.PreferenceSettings.HideNodesBelowSpecificConfidenceLevel ? FilteredHighConfidenceResults : results;
        }

        private MLNodeAutoCompletionResponse GetMLNodeAutocompleteResults(string requestJSON)
        {
            MLNodeAutoCompletionResponse results = null;
            try
            {
                var authProvider = dynamoViewModel.Model.AuthenticationManager.AuthProvider;
                if (!dynamoViewModel.IsIDSDKInitialized())
                {
                    throw new Exception("IDSDK missing or failed initialization.");
                }

                if (authProvider is IOAuth2AuthProvider oauth2AuthProvider && authProvider is IOAuth2AccessTokenProvider tokenprovider)
                {
                    var uri = DynamoUtilities.PathHelper.GetServiceBackendAddress(this, nodeAutocompleteMLEndpoint);
                    var client = new RestClient(uri);
                    var request = new RestRequest(string.Empty,Method.Post);
                    var tkn = tokenprovider?.GetAccessToken();
                    if (string.IsNullOrEmpty(tkn))
                    {
                        throw new Exception("Authentication required.");
                    }
                    request.AddHeader("Authorization",$"Bearer {tkn}");
                    request = request.AddJsonBody(requestJSON);
                    request.RequestFormat = DataFormat.Json;
                    RestResponse response = client.Execute(request);
                    //TODO maybe worth moving to system.text json in phases?
                    results = JsonConvert.DeserializeObject<MLNodeAutoCompletionResponse>(response.Content);
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
            // Save the filtered results for search.
            searchElementsCache = FilteredResults.ToList();
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
        internal void PopulateAutoCompleteCandidates()
        {
            if (PortViewModel == null) return;

            dynamoViewModel.CurrentSpaceViewModel.Model.NodeRemoved += NodeViewModel_Removed;
            ResetAutoCompleteSearchViewState();

            if (IsDisplayingMLRecommendation)
            {
                ShowNodeAutocompleMLResults();
                //Tracking Analytics when raising Node Autocomplete with the Recommended Nodes option selected (Machine Learning)
                Analytics.TrackEvent(
                    Actions.Show,
                    Categories.NodeAutoCompleteOperations,
                    nameof(NodeAutocompleteSuggestion.MLRecommendation));
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
                    PopulateDefaultAutoCompleteCandidates();
                }
                else
                {
                    FilteredResults = GetViewModelForNodeSearchElements(objectTypeMatchingElements);
                }
            }

            // Save the filtered results for search.
            searchElementsCache = FilteredResults.ToList();
        }

        internal void PopulateDefaultAutoCompleteCandidates()
        {
            if (PortViewModel.PortModel.PortType == PortType.Input)
            {
                switch (PortViewModel.PortModel.GetInputPortType())
                {
                    case "int":
                        FilteredResults = DefaultResults.Where(e => e.Name == "Number Slider" || e.Name == "Integer Slider").ToList();
                        break;
                    case "double":
                        FilteredResults = DefaultResults.Where(e => e.Name == "Number Slider" || e.Name == "Integer Slider").ToList();
                        break;
                    case "string":
                        FilteredResults = DefaultResults.Where(e => e.Name == "String").ToList();
                        break;
                    case "bool":
                        FilteredResults = DefaultResults.Where(e => e.Name == "Boolean").ToList();
                        break;
                    default:
                        FilteredResults = DefaultResults.Where(e => e.Name == "String" || e.Name == "Number Slider" || e.Name == "Integer Slider" || e.Name == "Number" || e.Name == "Boolean");
                        break;
                }
            }
            else
            {
                FilteredResults = DefaultResults.Where(e => e.Name == "Watch" || e.Name == "Watch 3D" || e.Name == "Python Script").ToList();
            }
        }

        internal void OnNodeAutoCompleteWindowClosed()
        {
            dynamoViewModel.CurrentSpaceViewModel.Model.NodeRemoved -= NodeViewModel_Removed;
        }

        internal void NodeViewModel_Removed(NodeModel node)
        {
            ParentNodeRemoved?.Invoke(node);
        }

        /// <summary>
        /// Returns a IEnumberable of NodeSearchElementViewModel for respective NodeSearchElements.
        /// </summary>
        private IEnumerable<NodeSearchElementViewModel> GetViewModelForNodeSearchElements(List<NodeSearchElement> searchElementsCache)
        {
            return searchElementsCache.Select(e =>
            {
                var vm = new NodeSearchElementViewModel(e, this);
                vm.RequestBitmapSource += SearchViewModelRequestBitmapSource;
                return vm;
            });
        }

        /// <summary>
        /// Returns a NodeSearchElementViewModel for a NodeSearchElement
        /// </summary>
        private NodeSearchElementViewModel GetViewModelForNodeSearchElement(NodeSearchElement nodeSearchElement)
        {
            if (nodeSearchElement != null)
            {
                var vm = new NodeSearchElementViewModel(nodeSearchElement, this);
                vm.RequestBitmapSource += SearchViewModelRequestBitmapSource;
                return vm;
            }
            return null;
        }


        /// <summary>
        ///     Performs a search using the given string as query and subset, if provided.
        /// </summary>
        /// <returns> Returns a list with a maximum MaxNumSearchResults elements.</returns>
        /// <param name="search"> The search query </param>
        internal IEnumerable<NodeSearchElementViewModel> SearchNodeAutocomplete(string search)
        {
            if (LuceneUtility != null)
            {
                //The DirectoryReader and IndexSearcher have to be assigned after commiting indexing changes and before executing the Searcher.Search() method, otherwise new indexed info won't be reflected
                LuceneUtility.dirReader = LuceneUtility.writer?.GetReader(applyAllDeletes: true);
                if (LuceneUtility.dirReader == null) return null;

                LuceneUtility.Searcher = new IndexSearcher(LuceneUtility.dirReader);

                string searchTerm = search.Trim();
                var candidates = new List<NodeSearchElementViewModel>();
                var parser = new MultiFieldQueryParser(LuceneConfig.LuceneNetVersion, LuceneConfig.NodeIndexFields, LuceneUtility.Analyzer)
                {
                    AllowLeadingWildcard = true,
                    DefaultOperator = LuceneConfig.DefaultOperator,
                    FuzzyMinSim = LuceneConfig.MinimumSimilarity
                };

                Query query = parser.Parse(LuceneUtility.CreateSearchQuery(LuceneConfig.NodeIndexFields, searchTerm));
                TopDocs topDocs = LuceneUtility.Searcher.Search(query, n: LuceneConfig.DefaultResultsCount);

                for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
                {
                    // read back a Lucene doc from results
                    Document resultDoc = LuceneUtility.Searcher.Doc(topDocs.ScoreDocs[i].Doc);

                    string name = resultDoc.Get(nameof(LuceneConfig.NodeFieldsEnum.Name));
                    string docName = resultDoc.Get(nameof(LuceneConfig.NodeFieldsEnum.DocName));
                    string cat = resultDoc.Get(nameof(LuceneConfig.NodeFieldsEnum.FullCategoryName));
                    string parameters = resultDoc.Get(nameof(LuceneConfig.NodeFieldsEnum.Parameters));


                    var foundNode = FindViewModelForNodeNameAndCategory(name, cat, parameters);
                    if (foundNode != null)
                    {
                        candidates.Add(foundNode);
                    }
                }

                return candidates;
            }
            return null;
        }

        /// <summary>
        /// Filters the matching node search elements based on user input in the search field. 
        /// </summary>
        internal void SearchAutoCompleteCandidates(string input)
        {
            if (PortViewModel == null) return;

            if (searchElementsCache.Any())
            {
                if (string.IsNullOrEmpty(input))
                {
                    FilteredResults = searchElementsCache;
                }
                else
                {
                    LuceneSearch.LuceneUtilityNodeAutocomplete = new LuceneSearchUtility(dynamoViewModel.Model, LuceneSearchUtility.DefaultStartConfig);

                    //Memory indexing process for Node Autocomplete (indexing just the nodes returned by the NodeAutocomplete service so we limit the scope of the query search)
                    var doc = LuceneUtility.InitializeIndexDocumentForNodes();
                    List<NodeSearchElement> nodeSearchElements = [.. searchElementsCache.Select(x => x.Model)];
                    LuceneUtility.AddNodeTypeToSearchIndex(nodeSearchElements, doc);

                    //Write the Lucene documents to memory
                    LuceneUtility.CommitWriterChanges();
                        
                    var luceneResults = SearchNodeAutocomplete(input);
                    var foundNodesModels = luceneResults.Select(x => x.Model);
                    var foundNodes = foundNodesModels.Select(MakeNodeSearchElementVM);

                    var filteredSearchElements = new List<NodeSearchElementViewModel>();

                    foreach (var node in foundNodes)
                    {
                        var matchingElement = searchElementsCache.FirstOrDefault(x => x.FullName.Equals(node.FullName));

                        if (matchingElement != null)
                        {
                            node.AutoCompletionNodeMachineLearningInfo = new AutoCompletionNodeMachineLearningInfo(matchingElement.AutoCompletionNodeMachineLearningInfo.DisplayIcon,
                                                                                                                   matchingElement.AutoCompletionNodeMachineLearningInfo.IsByRecommendation,
                                                                                                                   matchingElement.AutoCompletionNodeMachineLearningInfo.ConfidenceScore);
                            filteredSearchElements.Add(node);
                        }
                    }
                    FilteredResults = new List<NodeSearchElementViewModel>(filteredSearchElements).OrderBy(x => x.Name).ThenBy(x => x.Description);

                    LuceneUtility.DisposeWriter();
                }
            }
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
                parseResult = ParserUtils.ParseWithCore($"dummyName:{ portType};", core);
            }
            catch(ProtoCore.BuildHaltException)
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

            [Obsolete("This method will be removed in a future version of Dynamo")]
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
                    xTypeNames = xTypeNames.Select(type => (ParserUtils.ParseWithCore($"dummyName:{ type};", core).
                    CodeBlockNode.Children().ElementAt(0) as TypedIdentifierNode).datatype.Name);
                }
                if (y is ZeroTouchSearchElement yzt)
                {
                    yTypeNames = new string[] { yzt.Descriptor.ReturnType.Name };
                }
                // for non ZT nodes, we don't have concrete return types, so we need to parse the typename. 
                else
                {
                    yTypeNames = yTypeNames.Select(type => (ParserUtils.ParseWithCore($"dummyName:{ type};", core).
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
