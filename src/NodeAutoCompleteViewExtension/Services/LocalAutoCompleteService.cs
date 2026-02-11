using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Graph;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.NodeAutoComplete.ViewModels;
using DynamoUtilities;

namespace Dynamo.NodeAutoComplete.Services
{
    /// <summary>
    /// Service class for handling local autocomplete functionality using help files
    /// </summary>
    internal class LocalAutoCompleteService
    {
        private readonly DynamoViewModel dynamoViewModel;

        /// <summary>
        /// Constructor for LocalAutoCompleteService
        /// </summary>
        /// <param name="dynamoViewModel">The Dynamo view model instance</param>
        public LocalAutoCompleteService(DynamoViewModel dynamoViewModel)
        {
            this.dynamoViewModel = dynamoViewModel ?? throw new ArgumentNullException(nameof(dynamoViewModel));
        }

        /// <summary>
        /// Use local node help files to at least return one result that will work.
        /// Enhanced to support both built-in nodes and package nodes.
        /// </summary>
        /// <param name="selectedNode">The selected node</param>
        /// <param name="selectedPortModel">The selected port model</param>
        /// <returns>List of single result items from local help files</returns>
        public List<SingleResultItem> TryGetLocalAutoCompleteResult(NodeModel selectedNode, PortModel selectedPortModel)
        {
            if (selectedNode == null || selectedPortModel == null)
            {
                return new List<SingleResultItem>();
            }

            string nodeFullName = GetNodeFullName(selectedNode);
            
            // First try to find help files in package directories if the node belongs to a package
            var packageResult = TryGetPackageAutoCompleteResult(selectedNode, selectedPortModel, nodeFullName);
            if (packageResult.Any())
            {
                return packageResult;
            }

            // Fall back to built-in node help files
            return TryGetBuiltInAutoCompleteResult(selectedNode, selectedPortModel, nodeFullName);
        }

        /// <summary>
        /// Extract the full name from a node for help file lookup
        /// </summary>
        /// <param name="selectedNode">The node to extract the full name from</param>
        /// <returns>The full name of the node</returns>
        private string GetNodeFullName(NodeModel selectedNode)
        {
            switch (selectedNode)
            {
                case DSFunction dsFunction:
                    string fullSignature = dsFunction.FunctionSignature;
                    return fullSignature.Split('@')[0];
                case Function function:
                    var daName = $"{selectedNode.Category}.{selectedNode.GetOriginalName()}";
                    return $"{selectedNode.Category}.{selectedNode.GetOriginalName()}";
                default:
                    return selectedNode.GetType().ToString();
            }
        }

        /// <summary>
        /// Try to get autocomplete results from package help files
        /// </summary>
        /// <param name="selectedNode">The selected node</param>
        /// <param name="selectedPortModel">The selected port model</param>
        /// <param name="nodeFullName">The full name of the node</param>
        /// <returns>List of autocomplete results from package help files</returns>
        private List<SingleResultItem> TryGetPackageAutoCompleteResult(NodeModel selectedNode, PortModel selectedPortModel, string nodeFullName)
        {
            // Get the package manager extension
            var packageManagerExtension = dynamoViewModel.Model.GetPackageManagerExtension();
            if (packageManagerExtension?.PackageLoader == null)
            {
                return new List<SingleResultItem>();
            }

            // Try to find which package owns this node
            Package ownerPackage = null;
            
            // For custom nodes (Function type), check by file path
            if (selectedNode is Function function)
            {
                // Get the custom node info from the custom node manager
                if (dynamoViewModel.Model.CustomNodeManager.TryGetNodeInfo(function.Definition.FunctionId, out CustomNodeInfo customNodeInfo) 
                    && !string.IsNullOrEmpty(customNodeInfo.Path))
                {
                    ownerPackage = packageManagerExtension.PackageLoader.GetOwnerPackage(customNodeInfo.Path);
                }
            }
            // For DSFunction nodes (zero-touch), check by assembly path
            else if (selectedNode is DSFunction dsFunction)
            {
                ownerPackage = packageManagerExtension.PackageLoader.GetOwnerPackage(dsFunction.Controller.Definition.Assembly);
            }
            // For other DSFunctionBase nodes, check by assembly path
            else if (selectedNode is DSFunctionBase dsFunctionBase)
            {
                ownerPackage = packageManagerExtension.PackageLoader.GetOwnerPackage(dsFunctionBase.Controller.Definition.Assembly);
            }
            // For other nodes, try to find by type
            else
            {
                ownerPackage = packageManagerExtension.PackageLoader.GetOwnerPackage(selectedNode.GetType());
            }

            if (ownerPackage == null)
            {
                return new List<SingleResultItem>();
            }

            // Search for help files in package directories
            var searchDirectories = new List<string>();
            
            // Add package documentation directory
            if (Directory.Exists(ownerPackage.NodeDocumentaionDirectory))
            {
                searchDirectories.Add(ownerPackage.NodeDocumentaionDirectory);
            }
            
            // Add extra directory as fallback for sample files
            if (Directory.Exists(ownerPackage.ExtraDirectory))
            {
                searchDirectories.Add(ownerPackage.ExtraDirectory);
            }
            
            // Add root directory as fallback
            if (Directory.Exists(ownerPackage.RootDirectory))
            {
                searchDirectories.Add(ownerPackage.RootDirectory);
            }

            foreach (var searchDir in searchDirectories)
            {
                var result = SearchForHelpFileInDirectory(searchDir, nodeFullName, selectedNode, selectedPortModel);
                if (result.Any())
                {
                    return result;
                }
            }

            return new List<SingleResultItem>();
        }

        /// <summary>
        /// Try to get autocomplete results from built-in node help files (original logic)
        /// </summary>
        /// <param name="selectedNode">The selected node</param>
        /// <param name="selectedPortModel">The selected port model</param>
        /// <param name="nodeFullName">The full name of the node</param>
        /// <returns>List of autocomplete results from built-in help files</returns>
        private List<SingleResultItem> TryGetBuiltInAutoCompleteResult(NodeModel selectedNode, PortModel selectedPortModel, string nodeFullName)
        {
            var nodeHelpDocPath = new DirectoryInfo(Path.Combine(new FileInfo(Assembly.GetAssembly(typeof(DynamoModel)).Location).DirectoryName,
                Configurations.DynamoNodeHelpDocs));

            if (!nodeHelpDocPath.Exists)
            {
                return new List<SingleResultItem>();
            }

            return SearchForHelpFileInDirectory(nodeHelpDocPath.FullName, nodeFullName, selectedNode, selectedPortModel);
        }

        /// <summary>
        /// Search for help files in a specific directory and return autocomplete results
        /// </summary>
        /// <param name="searchDirectory">The directory to search in</param>
        /// <param name="nodeFullName">The full name of the node</param>
        /// <param name="selectedNode">The selected node</param>
        /// <param name="selectedPortModel">The selected port model</param>
        /// <returns>List of autocomplete results from the directory</returns>
        private List<SingleResultItem> SearchForHelpFileInDirectory(string searchDirectory, string nodeFullName, NodeModel selectedNode, PortModel selectedPortModel)
        {
            if (!Directory.Exists(searchDirectory))
            {
                return new List<SingleResultItem>();
            }

            var candidateFiles = FindCandidateHelpFiles(searchDirectory, nodeFullName, selectedNode);
            
            foreach (var filePath in candidateFiles)
            {
                try
                {
                    var result = ProcessHelpFile(filePath, selectedNode, selectedPortModel);
                    if (result.Any())
                    {
                        return result; // Return the first successful result
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but continue trying other files
                    dynamoViewModel.Model.Logger.Log($"Error processing help file {filePath}: {ex.Message}");
                }
            }

            return new List<SingleResultItem>();
        }

        /// <summary>
        /// Find all candidate help files that might contain relevant information
        /// </summary>
        /// <param name="searchDirectory">The directory to search in</param>
        /// <param name="nodeFullName">The full name of the node</param>
        /// <param name="selectedNode">The selected node</param>
        /// <returns>List of candidate file paths</returns>
        private List<string> FindCandidateHelpFiles(string searchDirectory, string nodeFullName, NodeModel selectedNode)
        {
            var candidates = new List<string>();
            
            // Primary candidate: exact nodeFullName match
            var primaryCandidate = Path.Combine(searchDirectory, $"{nodeFullName}.dyn");
            if (File.Exists(primaryCandidate))
            {
                candidates.Add(primaryCandidate);
            }

            // Secondary candidate: hashed filename
            var minimumQualifiedName = dynamoViewModel.GetMinimumQualifiedName(selectedNode);
            var hashedName = Hash.GetHashFilenameFromString(minimumQualifiedName);
            var hashedCandidate = Path.Combine(searchDirectory, $"{hashedName}.dyn");
            if (File.Exists(hashedCandidate) && !candidates.Contains(hashedCandidate))
            {
                candidates.Add(hashedCandidate);
            }

            return candidates;
        }

        /// <summary>
        /// Process a specific help file and extract autocomplete results
        /// </summary>
        /// <param name="filePath">The path to the help file</param>
        /// <param name="selectedNode">The selected node</param>
        /// <param name="selectedPortModel">The selected port model</param>
        /// <returns>List of autocomplete results from the help file</returns>
        private List<SingleResultItem> ProcessHelpFile(string filePath, NodeModel selectedNode, PortModel selectedPortModel)
        {
            // Early validation: check if file exists and is not empty
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists || fileInfo.Length == 0)
            {
                return new List<SingleResultItem>();
            }
            // Read and validate JSON content
            var fileContents = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(fileContents))
            {
                return new List<SingleResultItem>();
            }

            if (!PathHelper.isValidJson(filePath, out fileContents, out var exception))
            {
                dynamoViewModel.Model.Logger.LogError($"Could not read help file: {exception.Message}");
                return new List<SingleResultItem>();
            }

            // Quick check if the file likely contains our target node before expensive parsing
            if (!fileContents.Contains(selectedNode.GetOriginalName(), StringComparison.OrdinalIgnoreCase))
            {
                return new List<SingleResultItem>();
            }

            // Parse the workspace
            var workspace = WorkspaceModel.FromJson(fileContents, dynamoViewModel.Model.LibraryServices,
                dynamoViewModel.Model.EngineController, dynamoViewModel.Model.Scheduler,
                dynamoViewModel.Model.NodeFactory, false, false,
                dynamoViewModel.Model.CustomNodeManager,
                dynamoViewModel.Model.LinterManager);

            var matchingNode = workspace.Nodes
                .FirstOrDefault(n => n.GetOriginalName() == selectedNode.GetOriginalName());

            if (matchingNode == null) 
                return new List<SingleResultItem>();

            // Extract the prediction result
            return ExtractPredictionFromWorkspace(matchingNode, selectedPortModel);
        }

        /// <summary>
        /// Extract prediction result from a workspace containing the matching node
        /// </summary>
        /// <param name="matchingNode">The node that matches the selected node</param>
        /// <param name="selectedPortModel">The selected port model</param>
        /// <returns>List of prediction results</returns>
        private List<SingleResultItem> ExtractPredictionFromWorkspace(NodeModel matchingNode, PortModel selectedPortModel)
        {
            NodeModel predictedNode = null;
            PortModel matchingPortModel = null;

            switch (selectedPortModel.PortType)
            {
                case PortType.Input:
                    if (matchingNode.InPorts.Count > selectedPortModel.Index)
                    {
                        matchingPortModel = matchingNode.InPorts[selectedPortModel.Index];
                        if (matchingPortModel.Connectors.Any())
                        {
                            predictedNode = matchingPortModel.Connectors.First().Start.Owner;
                        }
                    }
                    break;
                case PortType.Output:
                    if (matchingNode.OutPorts.Count > selectedPortModel.Index)
                    {
                        matchingPortModel = matchingNode.OutPorts[selectedPortModel.Index];
                        if (matchingPortModel.Connectors.Any())
                        {
                            predictedNode = matchingPortModel.Connectors.First().End.Owner;
                        }
                    }
                    break;
            }

            if (predictedNode == null || predictedNode is CodeBlockNodeModel) 
                return new List<SingleResultItem>();
            
            var singleResultItem = new SingleResultItem(predictedNode, matchingPortModel.Index);
            if(string.IsNullOrEmpty(singleResultItem.CreationName))
            {
                return new List<SingleResultItem>();
            }
            return new List<SingleResultItem> { singleResultItem };
        }
    }
} 
