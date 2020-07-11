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
            this.ViewLoaded = viewLoadedParams;
        }

        internal static bool IsIronPythonPackageLoaded
        {
            get
            {
                try
                {
                    PythonEngineSelector.Instance.GetEvaluatorInfo(PythonEngineVersion.IronPython2,
                        out string evaluatorClass, out string evaluationMethod);
                    if (evaluatorClass == PythonEngineSelector.IronPythonEvaluatorClass &&
                        evaluationMethod == PythonEngineSelector.IronPythonEvaluationMethod)
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
                return false;
            }
        }

        internal static bool ContainsIronPythonDependencyInCurrentWS(WorkspaceModel workspace, ICustomNodeManager customNodeManager)
        {
            var containsIronPythonDependency = false;

            if (workspace == null)
                throw new ArgumentNullException(nameof(workspace));

            if (customNodeManager == null)
                throw new ArgumentNullException(nameof(customNodeManager));

            if (workspace.Nodes.Any(IsIronPythonNode))
            {
                containsIronPythonDependency = true;
            }

            // Check if any of the custom nodes has IronPython dependencies in it. 
            var customNodes = workspace.Nodes.OfType<Function>();

            if (CustomNodesContainIronPythonDependency(customNodes, customNodeManager))
            {
                containsIronPythonDependency = true;
            }

            return containsIronPythonDependency;
        }

        // This function returns true, if any of the custom nodes in the input list has an IronPython dependency. 
        // It traverses all CN's in the given list of customNodes and marks them as true in CustomNodePythonDependency, 
        // if the prarent custom node or any of its child custom nodes contain an IronPython dependency.
        private static bool CustomNodesContainIronPythonDependency(IEnumerable<Function> customNodes, ICustomNodeManager customNodeManager)
        {
            var containIronPythonDependency = false;

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
                    {
                        containIronPythonDependency = true;
                    }
                    continue;
                }

                var hasPythonNodesInCustomNodeWorkspace = customNodeWS.Nodes.Any(n => IsIronPythonNode(n));

                if (hasPythonNodesInCustomNodeWorkspace)
                {
                    CustomNodePythonDependencyMap.Add(customNodeWS.CustomNodeId, CNPythonDependencyType.DirectDependency);
                    containIronPythonDependency = true;
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
                        containIronPythonDependency = true;
                    }
                }
            }

            return containIronPythonDependency;
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
