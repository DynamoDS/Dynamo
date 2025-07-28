using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.PythonServices;
using PythonNodeModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.PythonMigration
{
    /// <summary>
    /// Class that helps with determining if a graph has specific Python dependencies and identifying them.
    /// </summary>
    public class GraphPythonDependencies
    {
        internal static readonly string PythonPackage = "DynamoIronPython2.7";
        // Current Dynamo inbuilt python package is CPython3. Change this when it is updated.
        internal static readonly string InBuiltPythonPackage = PythonEngineManager.CPython3EngineName;
        internal readonly Version PythonPackageVersion;
        private IWorkspaceModel workspace;
        private readonly ICustomNodeManager customNodeManager;

        /// <summary>
        /// A dictionary to mark Custom Nodes if they have a IronPython dependency or not.
        /// Data in this dictionary is required to display all the affected nodes in the IronPython warning modal.
        /// </summary>
        internal static Dictionary<Guid, CNPythonDependencyType> CustomNodePythonDependencyMap = new Dictionary<Guid, CNPythonDependencyType>();

        internal enum CNPythonDependencyType
        {
            NoDependency,
            NestedDependency,
            DirectDependency
        }

        internal GraphPythonDependencies(IWorkspaceModel workspaceModel, ICustomNodeManager customNodeManager, Version ironPythonTargetVersion)
        {
            workspace = workspaceModel;
            this.customNodeManager = customNodeManager;
            PythonPackageVersion = ironPythonTargetVersion;
        }

        internal void UpdateWorkspace(IWorkspaceModel workspaceModel)
        {
            this.workspace = workspaceModel;
        }

        private static bool IsIronPythonPackageLoaded()
        {
            return PythonEngineManager.Instance.AvailableEngines.Any(x => x.Name == PythonEngineManager.IronPython2EngineName);
        }

        /// <summary>
        /// Determines if the current workspace has any dependencies on IronPython engine.
        /// </summary>
        /// <returns>True if dependencies are found, false otherwise.</returns>
        internal bool CurrentWorkspaceHasIronPythonDependency()
        {
            var hasIronPythonDependency = false;

            // Check if any Python nodes in graph are using the IronPython engine
            if (this.workspace.Nodes.Any(IsIronPythonNode))
                hasIronPythonDependency = true;

            // Check if any of the custom nodes have IronPython dependencies 
            var customNodes = this.workspace.Nodes.OfType<Function>();
            if (CustomNodesHaveIronPythonDependency(customNodes, this.customNodeManager))
                hasIronPythonDependency = true;

            return hasIronPythonDependency;
        }

        /// <summary>
        /// Determines if the current workspace has any dependencies on CPython engine.
        /// </summary>
        /// <returns>True if dependencies are found, false otherwise.</returns>
        internal bool CurrentWorkspaceHasCPythonDependencies()
        {
            if (this.workspace == null)
                throw new ArgumentNullException(nameof(this.workspace));

            return this.workspace.Nodes.Any(IsCPythonNode);
        }

        internal IEnumerable<INodeLibraryDependencyInfo> AddPythonPackageDependency()
        {
            if (!CurrentWorkspaceHasIronPythonDependency())
                return null;

            var packageInfo = new PackageInfo(PythonPackage, PythonPackageVersion);
            var packageDependencyInfo = new PackageDependencyInfo(packageInfo);
            packageDependencyInfo.State = IsIronPythonPackageLoaded()
                ? PackageDependencyState.Loaded
                : PackageDependencyState.Missing;

            return new[] { packageDependencyInfo };
        }

        // Returns a dictionary of node and python engine mapping for the workspace python nodes.
        internal Dictionary<Guid, String> GetPythonEngineMapping()
        {
            var pythonNodeMapping = new Dictionary<Guid, String>();

            foreach (var node in workspace.Nodes.OfType<PythonNode>())
            {
                var enginePackage = node.EngineName.Equals(InBuiltPythonPackage) ? "InBuilt" : "FromPackage";
                pythonNodeMapping.Add(node.GUID, enginePackage);
            }

            return pythonNodeMapping;
        }

        /// <summary>
        /// This recursive function returns true if any of the custom nodes in the input list has an IronPython dependency.
        /// Any custom nodes in the input list traversed during evaluation have their dependencies cached in <see cref="CustomNodePythonDependencyMap"/>.
        /// Custom nodes are found to depend on IronPython if the parent custom node or any of its child custom nodes contain an IronPython dependency.
        /// </summary>
        /// <param name="customNodes">The custom nodes to evaluate.</param>
        /// <param name="customNodeManager">The custom node manager.</param>
        /// <returns>True if any IronPython dependencies are found, false otherwise.</returns>
        private static bool CustomNodesHaveIronPythonDependency(IEnumerable<Function> customNodes, ICustomNodeManager customNodeManager)
        {
            var hasIronPythonDependency = false;

            foreach (var customNode in customNodes)
            {
                if (!customNodeManager.TryGetFunctionWorkspace(customNode.FunctionSignature, false,
                    out ICustomNodeWorkspaceModel customNodeWS))
                    continue;

                // If a custom node workspace is already checked for IronPython dependencies, 
                // check the CustomNodePythonDependency dictionary instead of processing it again. 
                if (CustomNodePythonDependencyMap.TryGetValue(customNodeWS.CustomNodeId, out CNPythonDependencyType dependency))
                {
                    if (dependency == CNPythonDependencyType.DirectDependency || dependency == CNPythonDependencyType.NestedDependency)
                        hasIronPythonDependency = true;
                    continue;
                }

                var hasPythonNodesInCustomNodeWorkspace = customNodeWS.Nodes.Any(n => IsIronPythonNode(n));

                if (hasPythonNodesInCustomNodeWorkspace)
                {
                    CustomNodePythonDependencyMap.Add(customNodeWS.CustomNodeId, CNPythonDependencyType.DirectDependency);
                    hasIronPythonDependency = true;
                }
                else
                {
                    CustomNodePythonDependencyMap.Add(customNodeWS.CustomNodeId, CNPythonDependencyType.NoDependency);
                }

                // Recursively check for IronPython dependencies in the nested custom nodes.
                var nestedCustomNodes = customNodeWS.Nodes.OfType<Function>();

                if (nestedCustomNodes.Any())
                {
                    hasPythonNodesInCustomNodeWorkspace = CustomNodesHaveIronPythonDependency(nestedCustomNodes, customNodeManager);

                    // If a custom node contains an IronPython dependency in its sub-tree,
                    // update its corresponding value to 'NestedDependency' in CustomNodePythonDependency.
                    if (hasPythonNodesInCustomNodeWorkspace && CustomNodePythonDependencyMap[customNodeWS.CustomNodeId] != CNPythonDependencyType.DirectDependency)
                    {
                        CustomNodePythonDependencyMap[customNodeWS.CustomNodeId] = CNPythonDependencyType.NestedDependency;
                        hasIronPythonDependency = true;
                    }
                }
            }

            return hasIronPythonDependency;
        }

        internal static bool IsIronPythonNode(NodeModel obj)
        {
            if (!(obj is PythonNodeBase pythonNode))
                return false;

            return pythonNode.EngineName == PythonEngineManager.IronPython2EngineName;
        }

        internal static bool IsCPythonNode(NodeModel obj)
        {
            if (!(obj is PythonNodeBase pythonNode))
                return false;

            return pythonNode.EngineName == PythonEngineManager.CPython3EngineName;
        }
    }
}
