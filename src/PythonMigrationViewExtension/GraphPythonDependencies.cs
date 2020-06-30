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

        private static Dictionary<Guid, CNDependency> CustomNodePythonDependency = new Dictionary<Guid, CNDependency>();

        enum CNDependency
        {
            NoDependency,
            SubtreeDependency,
            DirectDependency
        }

        internal GraphPythonDependencies(ViewLoadedParams viewLoadedParams)
        {
            this.ViewLoaded = viewLoadedParams;
        }

        internal bool ContainsIronPythonDependencies()
        {
            var workspace = ViewLoaded.CurrentWorkspaceModel;

            if (workspace == null)
                throw new ArgumentNullException(nameof(workspace));

            if (workspace.Nodes.Any(n => IsIronPythonNode(n)))
            {
                return true;
            }

            // Check if any of the custom nodes has IronPython dependencies in it. 
            var customNodeManager = ViewLoaded.StartupParams.CustomNodeManager;
            var customNodes = workspace.Nodes.OfType<Function>();

            return CustomNodeContainsIronPythonDependency(customNodes, customNodeManager);           
        }

        // This function returns true, if any of the custom nodes in the input list has an IronPython dependency. 
        // It traverses all CN's in the given list of customNodes and marks them as true in CustomNodePythonDependency, 
        // if the prarent custom node or any of its child custom nodes contain an IronPython dependency.
        internal static bool CustomNodeContainsIronPythonDependency(IEnumerable<Function> customNodes, ICustomNodeManager customNodeManager)
        {
            var currentCNContainsPythonDependency = false;

            foreach (var customNode in customNodes)
            {
                customNodeManager.TryGetFunctionWorkspace(customNode.FunctionSignature, false, out ICustomNodeWorkspaceModel customNodeWS);

                // If a custom node workspace is already checked for IronPython dependencies, 
                // check the CustomNodePythonDependency dictionary instead of processing it again. 
                if (CustomNodePythonDependency.TryGetValue(customNodeWS.CustomNodeId, out CNDependency dependency))
                {
                    if (dependency == CNDependency.DirectDependency || dependency == CNDependency.SubtreeDependency)
                    {
                        currentCNContainsPythonDependency = true;
                    }
                    continue;
                }

                var hasPythonNodesInCustomNodeWorkspace = customNodeWS.Nodes.Any(n => IsIronPythonNode(n));

                if (hasPythonNodesInCustomNodeWorkspace)
                {
                    CustomNodePythonDependency.Add(customNodeWS.CustomNodeId, CNDependency.DirectDependency);
                    currentCNContainsPythonDependency = true;
                }
                else
                {
                    CustomNodePythonDependency.Add(customNodeWS.CustomNodeId, CNDependency.NoDependency);
                }

                // Recursively check for IronPython dependencies in the nested custom nodes.
                var nestedCustomNodes = customNodeWS.Nodes.OfType<Function>();

                if (nestedCustomNodes.Any())
                {
                    hasPythonNodesInCustomNodeWorkspace = CustomNodeContainsIronPythonDependency(nestedCustomNodes, customNodeManager);

                    // If a custom node contains an IronPython dependency in its sub-tree,
                    // update its corresponding value in CustomNodePythonDependency.
                    CustomNodePythonDependency.TryGetValue(customNodeWS.CustomNodeId, out dependency);
                    if (hasPythonNodesInCustomNodeWorkspace)
                    {
                        CustomNodePythonDependency[customNodeWS.CustomNodeId] = CNDependency.SubtreeDependency;
                    }

                    if (hasPythonNodesInCustomNodeWorkspace)
                    {
                        currentCNContainsPythonDependency = true;
                    }
                }
            }

            return currentCNContainsPythonDependency;
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
