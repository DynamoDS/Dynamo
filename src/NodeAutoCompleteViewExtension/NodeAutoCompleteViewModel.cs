using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Windows.Documents;
using Dynamo.Core;
using Dynamo.Engine;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Nodes;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels;
using Greg;
using Newtonsoft.Json;
using RestSharp;
using System.Windows;

namespace Dynamo.NodeAutoComplete
{
    public class NodeAutoCompleteViewModel : NotificationObject
    {
        internal UIElement DynamoView { get; set; }
        internal DynamoViewModel dynamoViewModel { get; set; }

        //constructor
        internal NodeAutoCompleteViewModel(UIElement dynamoView, DynamoViewModel dvm)
        {
            DynamoView = dynamoView;
            dynamoViewModel = dvm;
        }

        /*internal MLNodeAutoCompletionRequest GenerateRequestForMLAutocomplete()
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
                    var startPortName = (startNode is VariableInputNode || startNode is DSVarArgFunction) ? ParseVariableInputPortName(connector.Start.Name) : connector.Start.Name;
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
        }

        private MLNodeAutoCompletionResponse GetMLNodeAutocompleteResults(string requestJSON)
        {
            MLNodeAutoCompletionResponse results = null;
            var authProvider = dynamoViewModel.Model.AuthenticationManager.AuthProvider;

            if (authProvider is IOAuth2AuthProvider oauth2AuthProvider && authProvider is IOAuth2AccessTokenProvider tokenprovider)
            {
                try
                {
                    var uri = DynamoUtilities.PathHelper.GetServiceBackendAddress(this, nodeAutocompleteMLEndpoint);
                    var client = new RestClient(uri);
                    var request = new RestRequest(string.Empty, Method.Post);
                    var tkn = tokenprovider?.GetAccessToken();
                    if (string.IsNullOrEmpty(tkn))
                    {
                        throw new Exception("Authentication required.");
                    }
                    request.AddHeader("Authorization", $"Bearer {tkn}");
                    request = request.AddJsonBody(requestJSON);
                    request.RequestFormat = DataFormat.Json;
                    RestResponse response = client.Execute(request);
                    //TODO maybe worth moving to system.text json in phases?
                    results = JsonConvert.DeserializeObject<MLNodeAutoCompletionResponse>(response.Content);
                }
                catch (Exception ex)
                {
                    dynamoViewModel.Model.Logger.Log(ex.Message);
                    throw new Exception("Authentication failed.");
                }
            }

            return results;
        }

        // Rest API call to get the Node cluster Autocomlete results from the service.
        internal MLNodeClusterAutoCompletionResponse GetMLNodeClusterAutocompleteResults(string jsonRequest)
        {
            MLNodeClusterAutoCompletionResponse results = null;

            var authProvider = dynamoViewModel.Model.AuthenticationManager.AuthProvider;

            if (authProvider is IOAuth2AuthProvider oauth2AuthProvider && authProvider is IOAuth2AccessTokenProvider tokenprovider)
            {
                try
                {
                    var uri = DynamoUtilities.PathHelper.GetServiceBackendAddress(this, nodeClusterAutocompleteMLEndpoint);
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

                    //var jsonSerializerSettings = new JsonSerializerSettings();
                    //jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                    //TODO maybe worth moving to system.text json in phases?
                    results = JsonConvert.DeserializeObject<MLNodeClusterAutoCompletionResponse>(response.Content);
                }
                catch (Exception ex)
                {
                    dynamoViewModel.Model.Logger.Log(ex.Message);
                    throw new Exception("Authentication failed.");
                }
            }

            return results;
        }
    }*/
    }
}
