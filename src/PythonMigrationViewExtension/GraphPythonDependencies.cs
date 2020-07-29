using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Wpf.Extensions;
using PythonNodeModels;

namespace Dynamo.PythonMigration
{
    public class GraphPythonDependencies
    {
        private ViewLoadedParams ViewLoaded { get; set; }
        internal static readonly string PythonPackage = "DSIronPython_Test";
        internal static readonly Version PythonPackageVersion = new Version(1, 0, 8);


        // A dictionary to mark Custom Nodes if they have a IronPython dependency or not. 
        internal static Dictionary<Guid, CNPythonDependencyType> CustomNodePythonDependencyMap = new Dictionary<Guid, CNPythonDependencyType>();

        internal enum CNPythonDependencyType
        {
            NoDependency,
            NestedDependency,
            DirectDependency
        }

        internal GraphPythonDependencies(ViewLoadedParams viewLoadedParams)
        {
            ViewLoaded = viewLoadedParams;
        }

        private static bool IsIronPythonPackageLoaded()
        {
            PythonEngineSelector.Instance.GetEvaluatorInfo(
                PythonEngineVersion.IronPython2,
                out string evaluatorClass,
                out string evaluationMethod);

            return evaluatorClass == PythonEngineSelector.Instance.IronPythonEvaluatorClass
                && evaluationMethod == PythonEngineSelector.Instance.IronPythonEvaluationMethod;
        }

        /// <summary>
        /// Determines if the current workspace has any dependencies on IronPython
        /// </summary>
        /// <returns>True if depencies are found, false otherwise.</returns>
        internal bool CurrentWorkspaceHasIronPythonDepency()
        {
            var workspace = ViewLoaded.CurrentWorkspaceModel as WorkspaceModel;
            var customNodeManager = ViewLoaded.StartupParams.CustomNodeManager;

            // Check if any Python nodes in graph are using the IronPython engine
            if (workspace.Nodes.Any(IsIronPythonNode))
                return true;

            // Check if any of the custom nodes have IronPython dependencies 
            var customNodes = workspace.Nodes.OfType<Function>();
            if (CustomNodesContainIronPythonDependency(customNodes, customNodeManager))
                return true;

            return false;
        }

        internal IEnumerable<INodeLibraryDependencyInfo> AddPythonPackageDependency()
        {
            if (!CurrentWorkspaceHasIronPythonDepency())
                return null;

            var packageInfo = new PackageInfo(PythonPackage, PythonPackageVersion);
            var packageDependencyInfo = new PackageDependencyInfo(packageInfo);
            packageDependencyInfo.State = IsIronPythonPackageLoaded()
                ? PackageDependencyState.Loaded
                : PackageDependencyState.Missing;

            return new[] { packageDependencyInfo };
        }

        /// <summary>
        /// This recursive function returns true if any of the custom nodes in the input list has an IronPython dependency.
        /// Evaluation is short-circuited, so it returns true at the first encountered IronPython dependency.
        /// Any custom nodes in the input list traversed during evalution have their dependencies cached in <see cref="CustomNodePythonDependencyMap"/>.
        /// Custom odes are found to depend on IronPython if the parent custom node or any of its child custom nodes contain an IronPython dependency.
        /// </summary>
        /// <param name="customNodes">The custom nodes to evaluate.</param>
        /// <param name="customNodeManager">The custom node manager.</param>
        /// <returns>True if any IronPython depencies are found, false otherwise.</returns>
        private static bool CustomNodesContainIronPythonDependency(IEnumerable<Function> customNodes, ICustomNodeManager customNodeManager)
        {
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
                        return true;
                }

                var hasPythonNodesInCustomNodeWorkspace = customNodeWS.Nodes.Any(n => IsIronPythonNode(n));

                if (hasPythonNodesInCustomNodeWorkspace)
                {
                    CustomNodePythonDependencyMap.Add(customNodeWS.CustomNodeId, CNPythonDependencyType.DirectDependency);
                    return true;
                }
                else
                {
                    CustomNodePythonDependencyMap.Add(customNodeWS.CustomNodeId, CNPythonDependencyType.NoDependency);
                }

                // Recursively check for IronPython dependencies in the nested custom nodes.
                var nestedCustomNodes = customNodeWS.Nodes.OfType<Function>();

                if (nestedCustomNodes.Any())
                {
                    hasPythonNodesInCustomNodeWorkspace = CustomNodesContainIronPythonDependency(nestedCustomNodes, customNodeManager);

                    // If a custom node contains an IronPython dependency in its sub-tree,
                    // update its corresponding value to 'NestedDependency' in CustomNodePythonDependency.
                    if (hasPythonNodesInCustomNodeWorkspace && CustomNodePythonDependencyMap[customNodeWS.CustomNodeId] != CNPythonDependencyType.DirectDependency)
                    {
                        CustomNodePythonDependencyMap[customNodeWS.CustomNodeId] = CNPythonDependencyType.NestedDependency;
                        return true;
                    }
                }
            }

            return false;
        }

        internal bool ContainsCPythonDependencies()
        {
            var workspace = ViewLoaded.CurrentWorkspaceModel;
            if (workspace == null)
                throw new ArgumentNullException(nameof(workspace));

            return workspace.Nodes.Any(n => IsCPythonNode(n));
        }

        internal static bool IsIronPythonNode(NodeModel obj)
        {
            if (!(obj is PythonNodeBase pythonNode))
                return false;

            return pythonNode.Engine == PythonEngineVersion.IronPython2;
        }

        internal static bool IsCPythonNode(NodeModel obj)
        {
            if (!(obj is PythonNodeBase pythonNode))
                return false;

            return pythonNode.Engine == PythonEngineVersion.CPython3;
        }
    }
}
