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

            if (CustomNodesContainIronPythonDependencies(customNodes, customNodeManager))
            {
                return true;
            }

            return false;
        }

        internal bool CustomNodesContainIronPythonDependencies(IEnumerable<Function> customNodes, ICustomNodeManager customNodeManager)
        {
            ICustomNodeWorkspaceModel customNodeWS;

            foreach (var customNode in customNodes)
            {
                customNodeManager.TryGetFunctionWorkspace(customNode.FunctionSignature, false, out customNodeWS);

                if (customNodeWS.Nodes.Any(n => IsIronPythonNode(n)))
                {
                    return true;
                }

                // Recursively check for IronPython dependencies in the nested custom nodes. 
                var nestedCumtomNodes = customNodeWS.Nodes.OfType<Function>();
                if (CustomNodesContainIronPythonDependencies(nestedCumtomNodes, customNodeManager))
                {
                    return true;
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
