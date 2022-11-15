using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Properties;
using Dynamo.Search.SearchElements;
using Dynamo.Wpf.ViewModels;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Mirror;
using ProtoCore.Utils;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Text;
using Dynamo.Utilities;
using Dynamo.PackageManager;
using Dynamo.Core;
using RestSharp;
using Dynamo.Graph.Nodes.CustomNodes;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// Search View Model for Node AutoComplete Search Bar
    /// </summary>
    public class NodeAutoCompleteSearchViewModel : SearchViewModel
    {
        internal PortViewModel PortViewModel { get; set; }
        private List<NodeSearchElementViewModel> searchElementsCache;
        private string lowConfidenceMessageAdditional;
        private string noRecommendationsOrLowConfidenceMessage;
        private string noRecommendationsOrLowConfidenceTitle;
        private bool displayNoRecommendationsLowConfidence;
        private bool displayLowConfidence;
        private double confidenceThreshold = 0.5;

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
        /// The No Recommendations or Low Confidence Title
        /// </summary>
        public string NoRecommendationsOrLowConfidenceTitle
        {
            get { return noRecommendationsOrLowConfidenceTitle; }
            set
            {
                noRecommendationsOrLowConfidenceTitle = value;
                RaisePropertyChanged(nameof(NoRecommendationsOrLowConfidenceTitle));
            }
        }

        /// <summary>
        /// The No Recommendations or Low Confidence message
        /// </summary>
        public string NoRecommendationsOrLowConfidenceMessage
        {
            get { return noRecommendationsOrLowConfidenceMessage; }
            set
            {
                noRecommendationsOrLowConfidenceMessage = value;
                RaisePropertyChanged(nameof(NoRecommendationsOrLowConfidenceMessage));
            }
        }

        /// <summary>
        /// The Low Confidence additonal message
        /// </summary>
        public string LowConfidenceMessageAdditional
        {
            get { return lowConfidenceMessageAdditional; }
            set
            {
                lowConfidenceMessageAdditional = value;
                RaisePropertyChanged(nameof(LowConfidenceMessageAdditional));
            }
        }

        /// <summary>
        /// Indicates if display the No recommendations / Low confidence message (image and texts)
        /// </summary>
        public bool DisplayNoRecommendationsLowConfidence
        {
            get { return displayNoRecommendationsLowConfidence; }
            set
            {
                displayNoRecommendationsLowConfidence = value;
                RaisePropertyChanged(nameof(DisplayNoRecommendationsLowConfidence));
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dynamoViewModel">Dynamo ViewModel</param>
        internal NodeAutoCompleteSearchViewModel(DynamoViewModel dynamoViewModel) : base(dynamoViewModel)
        {
            // Off load some time consuming operation here
            InitializeDefaultAutoCompleteCandidates();
        }

        /// <summary>
        /// Reset Node AutoComplete search view state
        /// </summary>
        internal void ResetAutoCompleteSearchViewState()
        {
            if (!IsDisplayingMLRecommendation)
            {
                NoRecommendationsOrLowConfidenceMessage = string.Empty;
                NoRecommendationsOrLowConfidenceTitle = string.Empty;
                LowConfidenceMessageAdditional = string.Empty;
                DisplayNoRecommendationsLowConfidence = false;
                DisplayLowConfidence = false;
            }
            PopulateAutoCompleteCandidates();
        }

        private void InitializeDefaultAutoCompleteCandidates()
        {
            var candidates = new List<NodeSearchElementViewModel>();
            // TODO: These are basic input types in Dynamo
            // This should be only served as a temporary default case.
            var queries = new List<string>(){"String", "Number Slider", "Integer Slider", "Number", "Boolean", "Watch", "Watch 3D", "Python Script"};
            foreach (var query in queries)
            {
                var foundNode = Search(query).FirstOrDefault();
                if(foundNode != null)
                {
                    candidates.Add(foundNode);
                }
            }
            DefaultResults = candidates;
        }

        internal void DisplayMachineLearningResults()
        {
            // Intialize request for the the ML API
            MLNodeAutoCompletionRequest request = new MLNodeAutoCompletionRequest();
            DisplayNoRecommendationsLowConfidence = false;

            var nodeInfo = PortViewModel.PortModel.Owner;
            var portInfo = PortViewModel.PortModel;

            request.DynamoVersion = AssemblyHelper.GetDynamoVersion().ToString();
            request.NumberOfResults = 10;

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
            request.Port.Direction = portInfo.PortType == PortType.Input ? PortType.Input.ToString().ToLower() : PortType.Output.ToString().ToLower();
            request.Port.KeepListStructure = portInfo.KeepListStructure.ToString();
            request.Port.ListAtLevel = portInfo.Level;

            // Set host info
            request.Host.Name = string.IsNullOrEmpty(dynamoViewModel.Model.HostAnalyticsInfo.HostName) ? dynamoViewModel.Model.HostName : dynamoViewModel.Model.HostAnalyticsInfo.HostName;
            request.Host.Version = dynamoViewModel.Model.HostVersion;

            // Set packages info
            var packageManager = dynamoViewModel.Model.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();

            if (packageManager != null)
            {
                foreach (var pkg in packageManager.PackageLoader.LocalPackages)
                {
                    request.Packages = request.Packages.Append(new PackageItem(pkg.Name, pkg.VersionName));
                }
            }

            // Set context info
            // Considering upstream nodes when the PortType is output and considering downstream nodes when the PortType is input. 
            var connectors = nodeInfo.AllConnectors;

            foreach (var connector in connectors)
            {
                var revelantNode = PortViewModel.PortModel.PortType == PortType.Input ? connector.Start.Owner.GUID : connector.End.Owner.GUID;
                if (revelantNode.Equals(nodeInfo.GUID))
                {
                    // connectors info
                    var connectorRequest = new ConnectionsRequest
                    {
                        StartNode = new ConnectorNodeItem(connector.Start.Owner.GUID.ToString(), connector.Start.Name),
                        EndNode = new ConnectorNodeItem(connector.End.Owner.GUID.ToString(), connector.End.Name)
                    };
                    request.Context.Connections = request.Context.Connections.Append(connectorRequest);

                    // Add the relevant connecting node details to the context object. 
                    var nodeToAdd = PortViewModel.PortModel.PortType == PortType.Input ? connector.End.Owner : connector.Start.Owner;

                    var nodeRequest = new NodeRequest
                    {
                        Id = nodeToAdd.GUID.ToString()
                    };

                    if (nodeToAdd is DSFunctionBase node)
                    {
                        nodeRequest.Type.Id = node.CreationName;
                    }
                    else if (nodeToAdd is NodeModel nodeModel)
                    {
                        var typeID = new NodeModelTypeId(nodeModel.GetType().FullName, nodeModel.GetType().Assembly.GetName().Name);
                        nodeRequest.Type.Id = typeID.ToString();
                    }
                    request.Context.Nodes = request.Context.Nodes.Append(nodeRequest);
                }
            }

            string jsonRequest = JsonConvert.SerializeObject(request);
            MLNodeAutoCompletionResponse MLresults = null;

            // Get results from the ML API.
            try
            {
                MLresults = GetMLNodeAutocompleteResults(jsonRequest);
            }
            catch(Exception ex)
            {
                dynamoViewModel.Model.Logger.Log("Unable to fetch ML Node autocomplete results: " + ex.Message);
                DisplayNoRecommendationsLowConfidence = true;
                DisplayLowConfidence = false;
                NoRecommendationsOrLowConfidenceTitle = Resources.AutocompleteNoRecommendationsTitle;
                NoRecommendationsOrLowConfidenceMessage = Resources.AutocompleteNoRecommendationsMessage;
                return;
            }

            // no results
            if (MLresults == null || MLresults.Results.Count() == 0)
            {
                DisplayNoRecommendationsLowConfidence = true;
                DisplayLowConfidence = false;
                NoRecommendationsOrLowConfidenceTitle = Resources.AutocompleteNoRecommendationsTitle;
                NoRecommendationsOrLowConfidenceMessage = Resources.AutocompleteNoRecommendationsMessage;
                return;
            }

            var results = new List<NodeSearchElementViewModel>();
            FilteredResults = new List<NodeSearchElementViewModel>();

            var zeroTouchSearchElements = Model.SearchEntries.OfType<ZeroTouchSearchElement>().Where(x => x.IsVisibleInSearch);
            var nodeModelSearchElements = Model.SearchEntries.OfType<NodeModelSearchElement>().Where(x => x.IsVisibleInSearch);

            // ML Results are categorized based on the threshold confidence score before displaying. 
            if (MLresults.Results.Count() > 0)
            {
                FilteredHighConfidenceResults = new List<NodeSearchElementViewModel>();
                FilteredLowConfidenceResults = new List<NodeSearchElementViewModel>();

                foreach (var item in MLresults.Results)
                {
                    if (item.Node.Type.NodeType.Equals(Function.FunctionNode))
                    {
                        var element = zeroTouchSearchElements.FirstOrDefault(n => n.Descriptor.MangledName == item.Node.Type.Id);
                        var viewModelElement = GetViewModelForNodeSearchElement(element);

                        if (viewModelElement != null)
                        {
                            viewModelElement.AutoCompletionNodeMachineLearningInfo = new AutoCompletionNodeMachineLearningInfo(true, true, item.Score);
                            results.Add(viewModelElement);
                        }
                    }
                    else
                    {
                        // Retreive assembly name and full name from type id.
                        var typeInfo = GetInfoFromTypeId(item.Node.Type.Id);
                        string fullName = typeInfo.FullName;
                        string assemblyName = typeInfo.AssemblyName;

                        var nodesFromAssembly = nodeModelSearchElements.Where(n => Path.GetFileNameWithoutExtension(n.Assembly).Equals(assemblyName));
                        var element = nodesFromAssembly.FirstOrDefault(n => n.CreationName.Equals(fullName));
                        var viewModelElement = GetViewModelForNodeSearchElement(element);

                        if (viewModelElement != null)
                        {
                            viewModelElement.AutoCompletionNodeMachineLearningInfo = new AutoCompletionNodeMachineLearningInfo(true, true, item.Score);
                            results.Add(viewModelElement);
                        }
                    }
                }

                foreach (var result in results)
                {
                    if (result.AutoCompletionNodeMachineLearningInfo.ConfidenceScore >= confidenceThreshold)
                    {
                        FilteredHighConfidenceResults = FilteredHighConfidenceResults.Append(result);
                    }
                    else {
                        FilteredLowConfidenceResults = FilteredLowConfidenceResults.Append(result);
                    }
                }

                // Show low confidence section if there are some results under threshold.
                DisplayLowConfidence = FilteredLowConfidenceResults.Count() > 0;

                // Show low confidence page when all results are under threshold. 
                if (FilteredHighConfidenceResults.Count() <= 0)
                {
                    DisplayNoRecommendationsLowConfidence = true;
                    NoRecommendationsOrLowConfidenceTitle = Resources.AutocompleteLowConfidenceTitle;
                    NoRecommendationsOrLowConfidenceMessage = Resources.AutocompleteLowConfidenceMessage;
                    LowConfidenceMessageAdditional = Resources.AutocompleteLowConfidenceMessageAditional;
                    return;
                }

                // By default, show only the results which are above the threshold
                FilteredResults = FilteredHighConfidenceResults;
            }
        }

        private MLNodeAutoCompletionResponse GetMLNodeAutocompleteResults(string requestJSON)
        {
            MLNodeAutoCompletionResponse results = null;
            var uri = DynamoUtilities.PathHelper.getServiceBackendAddress(this, "MLNodeAutocomplete");
            var token = DynamoUtilities.PathHelper.getServiceConfigValues(this, "token");
            var authProvider = dynamoViewModel.Model.AuthenticationManager.AuthProvider;

            if (authProvider is IDSDKManager manager)
            {
                /*
                var client = new RestClient(uri);
                var request = new RestRequest(Method.POST);
                request = request.AddJsonBody(requestJSON) as RestRequest;
                request.RequestFormat = DataFormat.Json;

                try
                {
                    manager.SignRequest(ref request, client);
                    RestResponse response = client.Execute(request) as RestResponse;
                }
                catch (Exception e) {
                    throw new Exception("Error sign-in");
                }*/

                //var IDSDKtoken = manager.IDSDK_GetToken();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";

                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] byteArray = encoding.GetBytes(requestJSON);
                request.ContentLength = byteArray.Length;

                request.Headers.Add("Authorization", "Bearer " + token);

                Stream newStream = request.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    string jsonString = reader.ReadToEnd();
                    results = JsonConvert.DeserializeObject<MLNodeAutoCompletionResponse>(jsonString);
                }

                return results;
            }
            else
            {
                throw new Exception("Error fetting IDSDK token");
            }
        }

        /// <summary>
        /// Show the low confidence ML results.
        /// </summary>
        internal void ShowLowConfidenceResults()
        {
            DisplayLowConfidence = false;
            DisplayNoRecommendationsLowConfidence = false;
            IEnumerable<NodeSearchElementViewModel> allResults = FilteredHighConfidenceResults.Concat(FilteredLowConfidenceResults);
            FilteredResults = allResults;
            // Save the filtered results for search.
            searchElementsCache = FilteredResults.ToList();
        }

        // Full name and assembly name 
        private NodeModelTypeId GetInfoFromTypeId(string typeId)
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

        internal void PopulateAutoCompleteCandidates()
        {
            if (PortViewModel == null) return;

            FilteredResults = new List<NodeSearchElementViewModel>();
            FilteredHighConfidenceResults = new List<NodeSearchElementViewModel>();
            FilteredLowConfidenceResults = new List<NodeSearchElementViewModel>();

            if (IsDisplayingMLRecommendation)
            {
                DisplayMachineLearningResults();
            }
            else
            {
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
                    // Providing the saved search results to limit the scope of the query search.
                    // Then add back the ML info on filterted nodes as the Search function accepts elements of type NodeSearchElement
                    var foundNodes = Search(input, searchElementsCache.Select(x => x.Model));
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

            //gather all ztsearchelements that are visible in search and filter using inputPortType and zt return type name.
            var ztSearchElements = Model.SearchEntries.OfType<ZeroTouchSearchElement>().Where(x => x.IsVisibleInSearch);

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
                foreach (var element in Model.SearchEntries.OfType<NodeModelSearchElement>())
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
                        if (inputParameter.value.Type.ToString() == portType || DerivesFrom(inputParameter.value.Type.ToString(), portType, core))
                        {
                            ztSearchElement.AutoCompletionNodeElementInfo.PortToConnect = ztSearchElement.Descriptor.Type == FunctionType.InstanceMethod ? inputParameter.index + 1 : inputParameter.index;
                            elements.Add(ztSearchElement);
                            break;
                        }
                    }
                }

                // NodeModel nodes, match any output return type to inputport type name
                foreach (var element in Model.SearchEntries.OfType<NodeModelSearchElement>())
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
