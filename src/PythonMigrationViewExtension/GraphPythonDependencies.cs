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

        private Dictionary<Guid, bool> CustomNodePythonDependency = new Dictionary<Guid, bool>();

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

        internal bool CustomNodeContainsIronPythonDependency(IEnumerable<Function> customNodes, ICustomNodeManager customNodeManager)
        {
            ICustomNodeWorkspaceModel customNodeWS;

            foreach (var customNode in customNodes)
            {
                customNodeManager.TryGetFunctionWorkspace(customNode.FunctionSignature, false, out customNodeWS);

                // If a custom node workspace is already checked for Python dependencies, 
                // check the CustomNodePythonDependency dictionary instead of processing it again. 
                if (CustomNodePythonDependency.ContainsKey(customNodeWS.CustomNodeId))
                {
                    if (CustomNodePythonDependency[customNodeWS.CustomNodeId])
                    {
                        return true;
                    }
                    continue;
                }

                var hasPythonNodesInCustomNodeWorkspace = customNodeWS.Nodes.Any(n => IsIronPythonNode(n));
                CustomNodePythonDependency.Add(customNodeWS.CustomNodeId, hasPythonNodesInCustomNodeWorkspace);

                if (hasPythonNodesInCustomNodeWorkspace)
                {
                    return true;
                }

                // Recursively check for IronPython dependencies in the nested custom nodes.
                var nestedCustomNodes = customNodeWS.Nodes.OfType<Function>();

                if (nestedCustomNodes.Count() > 0)
                {
                    hasPythonNodesInCustomNodeWorkspace = CustomNodeContainsIronPythonDependency(nestedCustomNodes, customNodeManager);

                    // If a custom node contains a Python dependency in its sub-tree,
                    // update its corresponding value in CustomNodePythonDependency.
                    CustomNodePythonDependency.TryGetValue(customNodeWS.CustomNodeId, out bool temp);
                    if (temp != hasPythonNodesInCustomNodeWorkspace)
                    {
                        CustomNodePythonDependency[customNodeWS.CustomNodeId] = hasPythonNodesInCustomNodeWorkspace;
                    }

                    if (hasPythonNodesInCustomNodeWorkspace)
                    {
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
